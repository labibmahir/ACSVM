using Domain.Dto.HIKVision;
using Domain.Entities;
using Newtonsoft.Json;
using SurveillanceDevice.Integration.HttpClientBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using Utilities.Helpers;
using System.Net.Http.Headers;

namespace SurveillanceDevice.Integration.HIKVision
{
    public class HikVisionMachineService : IHikVisionMachineService
    {
        #region Private members
        private readonly ICustomHttpClientBuilder _clientBuilder;
        #endregion

        // TODO: Remove the client and keep the clientBuilder
        #region Constructor
        public HikVisionMachineService
        (
            ICustomHttpClientBuilder clientBuilder
        )
        {
            _clientBuilder = clientBuilder;
        }
        #endregion

        #region Person

        public async Task<VMUserInfoSearchResponse> GetAllUsers(Device device, int searchResultPosition, int maxResults)
        {
            var _client = _clientBuilder.GetCustomHttpClient(device.DeviceIP, Convert.ToInt16(device.Port), device.Username, device.Password);
            try
            {
                var searchData = new
                {
                    UserInfoSearchCond = new
                    {
                        searchID = "000000002",
                        searchResultPosition = searchResultPosition,
                        maxResults = maxResults,
                    }
                };
                var response = await _client.PostAsync("/ISAPI/AccessControl/UserInfo/Search?format=json",
                    new StringContent(JsonConvert.SerializeObject(searchData), Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();

                var responseText = await response.Content.ReadAsStringAsync();
                var res = System.Text.Json.JsonSerializer.Deserialize<VMUserInfoSearchResponseHolder>(responseText);

                if (res == null) throw (new Exception("Could not deserialize data"));
                return res.UserInfoSearch;
            }
            catch (Exception ex)
            {
                return new VMUserInfoSearchResponse();
            }
        }

        public async Task<int> GetUserCount(Device device)
        {
            try
            {
                VMUserCountResponseHolder res = new VMUserCountResponseHolder();
                var _client = _clientBuilder.GetCustomHttpClient(device.DeviceIP, Convert.ToInt16(device.Port), device.Username, device.Password);
                var response = await _client.GetAsync("/ISAPI/AccessControl/UserInfo/Count?format=json");
                response.EnsureSuccessStatusCode();

                var responseText = await response.Content.ReadAsStringAsync();
                res = System.Text.Json.JsonSerializer.Deserialize<VMUserCountResponseHolder>(responseText);
                if (res == null) throw (new Exception("Could not deserialize data"));
                return res.UserInfoCount.userNumber;
            }
            catch
            {
                return default;
                // throw;
            }
        }

        public async Task<string> AddUser(Device device, VMUserInfo user)
        {
            var _client = _clientBuilder.GetCustomHttpClient(device.DeviceIP, Convert.ToInt16(device.Port), device.Username, device.Password);
            try
            {
                var postData = new
                {
                    UserInfo = user
                };

                var jsonData = JsonConvert.SerializeObject(postData);

                var response = await _client.PostAsync("/ISAPI/AccessControl/UserInfo/Record?format=json",
                    new StringContent(jsonData, Encoding.UTF8, "application/json"));
                //response.EnsureSuccessStatusCode();

                var responseText = await response.Content.ReadAsStringAsync();
                return responseText;
            }
            catch (Exception ex)
            {
                // throw;
                return ex.Message;
            }
        }

        public async Task<string> UpdateUser(Device device, VMUserInfo user)
        {
            var _client = _clientBuilder.GetCustomHttpClient(device.DeviceIP, Convert.ToInt16(device.Port), device.Username, device.Password);

            try
            {
                user.addUser = false;

                var postData = new
                {
                    UserInfo = user
                };

                var jsonData = JsonConvert.SerializeObject(postData);

                var response = await _client.PutAsync("/ISAPI/AccessControl/UserInfo/Modify?format=json",
                   new StringContent(jsonData, Encoding.UTF8, "application/json"));

                response.EnsureSuccessStatusCode();

                var responseText = await response.Content.ReadAsStringAsync();
                return responseText;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> DeleteUserWithDetails(Device device, VMUserInfoDetailsDeleteRequest req)
        {
            var _client = _clientBuilder.GetCustomHttpClient(device.DeviceIP, Convert.ToInt16(device.Port), device.Username, device.Password);
            try
            {
                req.mode = "byEmployeeNo";

                var putData = new
                {
                    UserInfoDetail = req
                    //UserInfoDelCond = req
                };

                var jsonData = JsonConvert.SerializeObject(putData);

                var response = await _client.PutAsync("/ISAPI/AccessControl/UserInfoDetail/Delete?format=json",
                    new StringContent(jsonData, Encoding.UTF8, "application/json"));

                // response.EnsureSuccessStatusCode();

                // TODO: Check Deleting process
                var responseText = await response.Content.ReadAsStringAsync();
                return responseText;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<(string base64Image, byte[] binaryImage)> SaveEmployeeFaceImage(Device device, string employeeId, string faceUrl, string folderPath = null)
        {
            try
            {
                // Set authentication (Replace with your credentials)
                var _client = _clientBuilder.GetCustomHttpClient(device.DeviceIP, Convert.ToInt16(device.Port), device.Username, device.Password);

                // Fetch image
                HttpResponseMessage response = await _client.GetAsync(faceUrl);
                if (response.IsSuccessStatusCode)
                {
                    byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();

                    // Generate file path based on the folderPath or default to "faces/{employeeId}.jpg"
                    string filePath;
                    if (!string.IsNullOrEmpty(folderPath))
                    {
                        filePath = Path.Combine(folderPath, $"{employeeId}.jpg");
                        await File.WriteAllBytesAsync(filePath, imageBytes);
                    }

                    // Convert to Base64 (optional)
                    string base64Image = Convert.ToBase64String(imageBytes);

                    // Return both Base64 string and Binary image
                    return (base64Image, imageBytes);
                }
                else
                {
                    Console.WriteLine($"Failed to download image for {employeeId}: {response.StatusCode}");
                    return (null, null);  // Return null if download fails
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving image for {employeeId}: {ex.Message}");
                return (null, null);  // Return null in case of error
            }
        }
        #endregion Person

        #region FingerPrint
        public async Task<(bool IsSuccess, string Message)> SetFingerprint(Device device, VMFingerPrintSetUpRequest fingerInfo)
        {
            var _client = _clientBuilder.GetCustomHttpClient(device.DeviceIP, Convert.ToInt16(device.Port), device.Username, device.Password);
            try
            {
                fingerInfo.deleteFingerPrint = false;
                var postData = new
                {
                    FingerPrintCfg = fingerInfo
                };

                var response = await _client.PostAsync("/ISAPI/AccessControl/FingerPrint/SetUp?format=json",
                    new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json"));

                response.EnsureSuccessStatusCode();
                var responseText = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(responseText))
                {
                    VMFingerPrintSetUpResponseHolder fr = JsonConvert.DeserializeObject<VMFingerPrintSetUpResponseHolder>(responseText);
                    foreach (VMFingerPrintSetUpStatusListItem item in fr.FingerPrintStatus.StatusList)
                    {
                        if (item.id == fingerInfo.enableCardReader[0]) // TODO: Fix this
                        {
                            if (item.cardReaderRecvStatus != 1)
                            {
                                if (item.cardReaderRecvStatus == 0)
                                    return (false, "Connection Failure.");
                                else if (item.cardReaderRecvStatus == 2)
                                    return (false, "the Finger Print Module is Offline.");
                                else if (item.cardReaderRecvStatus == 3)
                                    return (false, "the Fnger Print quality is poor, try again.");
                                else if (item.cardReaderRecvStatus == 4)
                                    return (false, "the Memomory is full.");

                                else if (item.cardReaderRecvStatus == 5)
                                    return (false, "the Fnger Print already exists.");

                                else if (item.cardReaderRecvStatus == 6)
                                    return (false, "the Fnger Print ID already exists.");
                                else
                                    return (false, "Something went wrong");
                            }
                            else
                            {
                                return (true, "Fingerprint setup was successful.");
                            }
                        }
                    }
                }
                else
                {
                    return (false, "No response received from the device.");
                }

                return (false, "Fingerprint status could not be determined.");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }
        public async Task<VMCaptureFingerPrintResponse> CaptureFingerPrint(Device device, VMCaptureFingerPrintRequest req)
        {
            try
            {
                var _client = _clientBuilder.GetCustomHttpClient(device.DeviceIP, Convert.ToInt16(device.Port), device.Username, device.Password);
                _client.DefaultRequestHeaders.Add("Accept", "application/xml");
                string xml = "";
                XmlSerializer serializer = new XmlSerializer(typeof(VMCaptureFingerPrintRequest));
                await using (var stringWriter = new Utf8StringWriter())
                {
                    await using (XmlWriter writer = XmlWriter.Create(stringWriter, new XmlWriterSettings() { Async = true }))
                    {
                        serializer.Serialize(writer, req);
                        xml = stringWriter.ToString();
                    }
                }

                var response = await _client.PostAsync("/ISAPI/AccessControl/CaptureFingerPrint",
                    new StringContent(xml, Encoding.UTF8, "application/xml"));

                response.EnsureSuccessStatusCode();

                var responseText = await response.Content.ReadAsStringAsync();
                var buffer = Encoding.UTF8.GetBytes(responseText);
                VMCaptureFingerPrintResponse res = new VMCaptureFingerPrintResponse();
                //string? s = "<CaptureFingerPrint version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\"><fingerData>MzAxGA2lJSi8hxvhJCiUhC7lJUjYhDr5JWjMDjgtJVjIgFn1JEiseWmdFSi4AmrVJTjQgnCBJSigg3lVJTiUDI3hJSishb4JJkjch7IFJbjAc8T9FMiUX8tlJajUddHVFLi43+R9JWjJZQOeFci8GJmVFJikZ/idFJjgOfJ5FXi+DbQFFzjIFfwhF2jAiCjOJDjIKgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAYRMAGysSY4EKhSEHMgPAC4cyDyEA3gkMO0dCUBAIgwIlMBGTB4KUHzDk3AYCORsQUKAOdXhKUnHZDHP7NjQQhA1fl2owwckQZZcoTeKaEmeUVyEExw/ftW0h0o0FeThZRsKcFA37aD5zcgMOt1oVxq4RGDoWMXHJBIB8CCM0vRM5YhUjBM4AhwAAAAAAoW0=</fingerData><fingerNo>1</fingerNo><fingerPrintQuality>97</fingerPrintQuality></CaptureFingerPrint>";

                using (var stream = new MemoryStream(buffer))
                {
                    var ser = new XmlSerializer(typeof(VMCaptureFingerPrintResponse));
                    res = (VMCaptureFingerPrintResponse)ser.Deserialize(stream);
                }
                if (res != null && res.fingerData != null)
                {
                    return res;
                }
                else
                {
                    throw (new Exception("Could not read fingerprint data"));
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> DeleteFingerprint(Device device, EmployeeDetailDto request)
        {
            var _client = _clientBuilder.GetCustomHttpClient(device.DeviceIP, Convert.ToInt16(device.Port), device.Username, device.Password);

            try
            {
                VMFingerPrintDeleteRequest req = new VMFingerPrintDeleteRequest();

                req.mode = "byEmployeeNo";

                VMFingerPrintEmployeeNumberDetail vMFingerPrintEmployeeNumberDetail = new VMFingerPrintEmployeeNumberDetail()
                {
                    employeeNo = request.EmployeeNo,
                    fingerPrintID = new int[] { request.FingerPrintNumber }
                };

                req.EmployeeNoDetail = vMFingerPrintEmployeeNumberDetail;

                var postData = new
                {
                    FingerPrintDelete = req
                };

                var jsonData = JsonConvert.SerializeObject(postData);

                var response = await _client.PutAsync("/ISAPI/AccessControl/FingerPrint/Delete?format=json",
                    new StringContent(jsonData, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();

                var responseText = await response.Content.ReadAsStringAsync();

                if (responseText != string.Empty)
                {
                    VMResponseStatus? rs = JsonConvert.DeserializeObject<VMResponseStatus?>(responseText);
                    if (rs == null)
                    {
                        //_logger.LogError("An error happend while deleteing a finger print");
                        //throw new Exception("An error happend while deleteing a finger print");
                        return false;
                    }
                    if (rs.statusCode.Equals("1"))
                    {
                        bool ans = await CheckFingerprintDeleteProcess(_client);
                        return ans;
                    }
                    else return false;
                }
                else
                {
                    //_logger.LogError("An error happend while deleteing a finger print");
                    //throw new Exception("An error happend while deleteing a finger print");
                    return false;
                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public async Task<bool> CheckFingerprintDeleteProcess(HttpClient _client)
        {
            while (true)
            {
                try
                {
                    var response = await _client.GetAsync("/ISAPI/AccessControl/FingerPrint/DeleteProcess?format=json");
                    response.EnsureSuccessStatusCode();
                    var responseText = await response.Content.ReadAsStringAsync();
                    if (responseText != string.Empty)
                    {
                        VMFingerPrintDeleteProcessHolder? fd = JsonConvert.DeserializeObject<VMFingerPrintDeleteProcessHolder?>(responseText);
                        if (fd.FingerPrintDeleteProcess.status.Equals("success")) return true;
                        else return false;
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async Task<VMFingerPrintSearchResponse> GetFingerprintsByEmployeeId(Device device, string EmployeeId)
        {
            var _client = _clientBuilder.GetCustomHttpClient(device.DeviceIP, Convert.ToInt16(device.Port), device.Username, device.Password);
            try
            {
                var searchData = new
                {
                    FingerPrintCond = new
                    {
                        searchID = "000000001",
                        employeeNo = EmployeeId,
                        cardReaderNo = 1 // TODO: Figure out this
                    }
                };
                var response = await _client.PostAsync("/ISAPI/AccessControl/FingerPrintUpload?format=json",
                    new StringContent(JsonConvert.SerializeObject(searchData), Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();

                var responseText = await response.Content.ReadAsStringAsync();
                var fingerPrintInfo = System.Text.Json.JsonSerializer.Deserialize<VMFingerPrintSearchByEmployeeResponse>(responseText);
                if (fingerPrintInfo == null) throw (new Exception("Could not deserialize data"));
                return fingerPrintInfo.FingerPrintInfo;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        #endregion FingerPrint
        #region Card
        public async Task<(bool Success, string Message)> AddCard(Device device, VMCardInfo card)
        {
            var _client = _clientBuilder.GetCustomHttpClient(device.DeviceIP, Convert.ToInt16(device.Port), device.Username, device.Password);
            try
            {
                var postData = new
                {
                    CardInfo = card
                };

                var response = await _client.PostAsync("/ISAPI/AccessControl/CardInfo/Record?format=json",
                    new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    return (true, responseText); // Return success with the response message
                }
                else
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    return (false, $"Failed to add card. Status code: {response.StatusCode}, Response: {errorText}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error when adding card: {ex.Message}"); // Return failure with the error message
            }
        }

        public async Task<VMCardInfoSearchResponse> GetCardsByEmployees(Device device, List<VMEmployeeNoListItem> employeeNoList, int searchResultPosition, int maxResults)
        {
            var _client = _clientBuilder.GetCustomHttpClient(device.DeviceIP, Convert.ToInt16(device.Port), device.Username, device.Password);
            try
            {
                var searchData = new
                {
                    CardInfoSearchCond = new
                    {
                        searchID = "000000001",
                        searchResultPosition = searchResultPosition,
                        maxResults = maxResults,
                        EmployeeNoList = employeeNoList
                    }
                };
                var temp123 = JsonConvert.SerializeObject(searchData);
                var response = await _client.PostAsync("/ISAPI/AccessControl/CardInfo/Search?format=json",
                    new StringContent(JsonConvert.SerializeObject(searchData), Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();

                var responseText = await response.Content.ReadAsStringAsync();
                var res = System.Text.Json.JsonSerializer.Deserialize<VMCardInfoSearchResponse>(responseText);
                if (res == null) throw (new Exception("Could not deserialize data"));
                return res;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<EmployeeCardCount> GetEmployeeCardCount(Device device, string EmployeeId)
        {
            var _client = _clientBuilder.GetCustomHttpClient(device.DeviceIP, Convert.ToInt16(device.Port), device.Username, device.Password);
            try
            {
                var response = await _client.GetAsync($"/ISAPI/AccessControl/CardInfo/Count?format=json&employeeNo={EmployeeId}");
                response.EnsureSuccessStatusCode();

                var responseText = await response.Content.ReadAsStringAsync();
                var res = System.Text.Json.JsonSerializer.Deserialize<VMCardInfoCountResponse>(responseText);
                if (res == null) throw (new Exception("Could not deserialize data"));
                EmployeeCardCount result = new EmployeeCardCount(res, EmployeeId);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<(bool Success, string Message)> DeleteCard(Device device, VMCardInfoDeleteRequest req)
        {
            var _client = _clientBuilder.GetCustomHttpClient(device.DeviceIP, Convert.ToInt16(device.Port), device.Username, device.Password);
            try
            {
                var putData = new
                {
                    CardInfoDelCond = req
                };

                var response = await _client.PutAsync("/ISAPI/AccessControl/CardInfo/Delete?format=json",
                    new StringContent(JsonConvert.SerializeObject(putData), Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    return (true, responseText); // Return success with the response message
                }
                else
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    return (false, $"Failed to delete card. Status code: {response.StatusCode}, Response: {errorText}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error when deleting card: {ex.Message}"); // Return failure with the error message
            }
        }
        #endregion Card


        #region face
        public async Task<(bool IsSuccess, string Message)> PostFaceRecordToLibrary(string ip, int port, string username, string password, FacePictureUploadDto faceRecordRequest, byte[] faceImage)
        {
            var _client = _clientBuilder.GetCustomHttpClient(ip, port, username, password);
            _client.Timeout = TimeSpan.FromMilliseconds(30000);

            //string jsonMetadata = "{\"faceLibType\":\"blackFD\",\"FDID\":\"1\",\"FPID\":\"ETL24020571\"}"; // Preformatted for testing

            var jsonMetadata = System.Text.Json.JsonSerializer.Serialize(faceRecordRequest);

            // string url = $"http://{ip}:{port}/ISAPI/Intelligent/FDLib/FaceDataRecord?format=json";
            string url = $"/ISAPI/Intelligent/FDLib/FaceDataRecord?format=json";

            try
            {
                var formData = new MultipartFormDataContent("----MyBoundary")
                {
                    new StringContent(jsonMetadata, Encoding.UTF8, "application/json")
                    {
                        Headers = { ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "FaceDataRecord" } }
                    },
                    new ByteArrayContent(faceImage)
                    {
                        Headers =
                        {
                            ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                            {
                                Name = "FaceImage",
                                FileName = "facePic.jpg"
                            },
                            ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg")
                        }
                    }
                };

                formData.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/form-data");
                formData.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", "----MyBoundary"));

                var response = await _client.PostAsync(url, formData);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    return (true, responseBody);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, errorContent);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Exception occurred: {ex.Message}");
            }
        }

        public async Task<(bool IsSuccess, string Message)> DeleteFaceRecordToLibrary(string ip, int port, string username, string password, FacePictureRemoveDto faceRecordRequest, byte[] faceImage)
        {
            var _client = _clientBuilder.GetCustomHttpClient(ip, port, username, password);
            _client.Timeout = TimeSpan.FromMilliseconds(30000);

            //string jsonMetadata = "{\"faceLibType\":\"blackFD\",\"FDID\":\"1\",\"FPID\":\"ETL24020571\"}"; // Preformatted for testing

            var jsonMetadata = System.Text.Json.JsonSerializer.Serialize(faceRecordRequest);

            // string url = $"http://{ip}:{port}/ISAPI/Intelligent/FDLib/FaceDataRecord?format=json";
            string url = $"/ISAPI/Intelligent/FDLib/FDSetUp?format=json";

            try
            {
                var formData = new MultipartFormDataContent("----MyBoundary")
                {
                    new StringContent(jsonMetadata, Encoding.UTF8, "application/json")
                    {
                        Headers = { ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "FaceDataRecord" } }
                    },
                    new ByteArrayContent(faceImage)
                    {
                        Headers =
                        {
                            ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                            {
                                Name = "FaceImage",
                                FileName = "facePic.jpg"
                            },
                            ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg")
                        }
                    }
                };

                formData.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/form-data");
                formData.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", "----MyBoundary"));

                var response = await _client.PutAsync(url, formData);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    return (true, responseBody);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, errorContent);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Exception occurred: {ex.Message}");
            }
        }
        #endregion
        #region Event
        public async Task<int> GetAcsEventCount(Device device, DateTime startTime, DateTime endTime)
        {
            try
            {
                var _client = _clientBuilder.GetCustomHttpClient(device.DeviceIP, Convert.ToInt16(device.Port), device.Username, device.Password);

                var searchData = new
                {
                    AcsEventTotalNumCond = new
                    {
                        searchID = "000000002",
                        searchResultPosition = "0",
                        maxResults = 20,
                        startTime = startTime.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                        endTime = endTime.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                        major = 0,
                        minor = 0
                    }
                };
                var response = await _client.PostAsync("/ISAPI/AccessControl/AcsEventTotalNum?format=json",
                    new StringContent(JsonConvert.SerializeObject(searchData), Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();

                var responseText = await response.Content.ReadAsStringAsync();
                var res = System.Text.Json.JsonSerializer.Deserialize<VMAcsEventTotalNumResponseHolder>(responseText);

                if (res == null) throw (new Exception("Could not deserialize data"));
                return res.AcsEventTotalNum.totalNum;
            }
            catch
            {
                return default;
                // throw;
            }
        }
        public async Task<AcsEventResponse> GetAcsEvent(Device device, DateTime startTime, DateTime endTime, int searchResultPosition, int maxResults)
        {
            try
            {
                var _client = _clientBuilder.GetCustomHttpClient(device.DeviceIP, Convert.ToInt16(device.Port), device.Username, device.Password);

                var searchData = new
                {
                    AcsEventCond = new
                    {
                        searchID = "000000002",
                        searchResultPosition = searchResultPosition,
                        maxResults = maxResults,
                        startTime = startTime.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                        endTime = endTime.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                        major = 0,
                        minor = 0
                    }
                };

                var response = await _client.PostAsync("/ISAPI/AccessControl/AcsEvent?format=json",
                    new StringContent(JsonConvert.SerializeObject(searchData), Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();

                var responseText = await response.Content.ReadAsStringAsync();
                var res = System.Text.Json.JsonSerializer.Deserialize<AcsEventResponse>(responseText);

                if (res == null) throw (new Exception("Could not deserialize data"));
                return res;
            }
            catch
            {
                return default;
                // throw;
            }
        }

        #endregion
    }
}
