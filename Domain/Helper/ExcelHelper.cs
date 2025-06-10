using ClosedXML.Excel;
using Domain.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Constants;

namespace Domain.Helper
{
    public static class ExcelHelper
    {

        public static PersonExcelImportResult ReadPersonsFromExcel(Stream stream)
        {
            var result = new PersonExcelImportResult();
            var personNumberSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header

            int rowIndex = 2;

            foreach (var row in rows)
            {
                try
                {
                    var personNumber = row.Cell(3).GetString().Trim();

                    // Check for duplicate PersonNumber
                    if (!string.IsNullOrWhiteSpace(personNumber))
                    {
                        if (!personNumberSet.Add(personNumber))
                        {
                            result.ValidationErrors.Add(new PersonExcelImportValidations
                            {
                                RowNo = rowIndex,
                                Message = $"Duplicate PersonNumber '{personNumber}' found."
                            });

                            rowIndex++;
                            continue;
                        }
                    }

                    var accessLevels = !string.IsNullOrEmpty(row.Cell(12).GetString())
                        ? row.Cell(12).GetString().Split(',').Select(int.Parse).ToArray()
                        : [0];

                    var deviceIds = !string.IsNullOrEmpty(row.Cell(13).GetString())
                        ? row.Cell(13).GetString().Split(',').Select(int.Parse).ToArray()
                        : [0];

                    // Business rule: both AccessLevelIds and DeviceIdList cannot be [0]
                    if (accessLevels.Length == 1 && accessLevels[0] == 0 &&
                        deviceIds.Length == 1 && deviceIds[0] == 0)
                    {
                        result.ValidationErrors.Add(new PersonExcelImportValidations
                        {
                            RowNo = rowIndex,
                            Message = "Both AccessLevelIds and DeviceIdList are empty or invalid."
                        });

                        rowIndex++;
                        continue;
                    }

                    var person = new PersonDto
                    {
                        Oid = Guid.NewGuid(),
                        FirstName = row.Cell(1).GetString(),
                        Surname = row.Cell(2).GetString(),
                        PersonNumber = personNumber,
                        Department = row.Cell(4).GetString(),
                        Email = row.Cell(5).GetString(),
                        PhoneNumber = row.Cell(6).GetString(),
                        Gender = Enum.Parse<Enums.Gender>(row.Cell(7).GetString(), true),
                        IsDeviceAdministrator = row.Cell(8).GetBoolean(),
                        ValidateStartPeriod = DateTime.Parse(row.Cell(9).GetString()),
                        ValidateEndPeriod = string.IsNullOrEmpty(row.Cell(10).GetString()) ? null : DateTime.Parse(row.Cell(10).GetString()),
                        OrganizationId = int.TryParse(row.Cell(11).GetString(), out var orgId) ? orgId : null,
                        AccessLevelIds = accessLevels,
                        DeviceIdList = deviceIds,
                        UserVerifyMode = Enum.TryParse<Enums.UserVerifyMode>(row.Cell(14).GetString(), true, out var mode) ? mode : null
                    };

                    result.Persons.Add(person);
                }
                catch (Exception ex)
                {
                    result.ValidationErrors.Add(new PersonExcelImportValidations
                    {
                        RowNo = rowIndex,
                        Message = ex.Message
                    });
                }

                rowIndex++;
            }

            return result;
        }
        public static PersonExcelImportResult ReadPersonsFromCsv(Stream stream)
        {
            var result = new PersonExcelImportResult();
            var personNumberSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using var reader = new StreamReader(stream);
            int rowIndex = 0;

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                rowIndex++;

                // Skip header
                if (rowIndex == 1)
                    continue;

                try
                {
                    var columns = line.Split(',');

                    if (columns.Length < 14)
                    {
                        result.ValidationErrors.Add(new PersonExcelImportValidations
                        {
                            RowNo = rowIndex,
                            Message = "Missing columns"
                        });
                        continue;
                    }

                    var personNumber = columns[2].Trim();

                    // Check duplicate PersonNumber
                    if (!string.IsNullOrWhiteSpace(personNumber))
                    {
                        if (!personNumberSet.Add(personNumber))
                        {
                            result.ValidationErrors.Add(new PersonExcelImportValidations
                            {
                                RowNo = rowIndex,
                                Message = $"Duplicate PersonNumber '{personNumber}' found."
                            });
                            continue;
                        }
                    }

                    var accessLevels = !string.IsNullOrEmpty(columns[11])
                        ? columns[11].Split(',').Select(int.Parse).ToArray()
                        : [0];

                    var deviceIds = !string.IsNullOrEmpty(columns[12])
                        ? columns[12].Split(',').Select(int.Parse).ToArray()
                        : [0];

                    if (accessLevels.Length == 1 && accessLevels[0] == 0 &&
                        deviceIds.Length == 1 && deviceIds[0] == 0)
                    {
                        result.ValidationErrors.Add(new PersonExcelImportValidations
                        {
                            RowNo = rowIndex,
                            Message = "Both AccessLevelIds and DeviceIdList are empty or invalid."
                        });
                        continue;
                    }

                    var person = new PersonDto
                    {
                        Oid = Guid.NewGuid(),
                        FirstName = columns[0].Trim(),
                        Surname = columns[1].Trim(),
                        PersonNumber = personNumber,
                        Department = columns[3].Trim(),
                        Email = columns[4].Trim(),
                        PhoneNumber = columns[5].Trim(),
                        Gender = Enum.Parse<Enums.Gender>(columns[6].Trim(), true),
                        IsDeviceAdministrator = bool.Parse(columns[7].Trim()),
                        ValidateStartPeriod = DateTime.Parse(columns[8].Trim()),
                        ValidateEndPeriod = string.IsNullOrWhiteSpace(columns[9]) ? null : DateTime.Parse(columns[9].Trim()),
                        OrganizationId = int.TryParse(columns[10], out var orgId) ? orgId : null,
                        AccessLevelIds = accessLevels,
                        DeviceIdList = deviceIds,
                        UserVerifyMode = Enum.TryParse(columns[13].Trim(), true, out Enums.UserVerifyMode mode) ? mode : null
                    };

                    result.Persons.Add(person);
                }
                catch (Exception ex)
                {
                    result.ValidationErrors.Add(new PersonExcelImportValidations
                    {
                        RowNo = rowIndex,
                        Message = ex.Message
                    });
                }
            }

            return result;
        }



    }
}
