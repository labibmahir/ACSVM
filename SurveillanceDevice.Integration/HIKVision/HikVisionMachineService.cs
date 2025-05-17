using Domain.Dto.HIKVision;
using Domain.Entities;
using Newtonsoft.Json;
using SurveillanceDevice.Integration.HttpClientBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        #endregion Card
    }
}
