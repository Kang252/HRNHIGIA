using NHIGIA.Common.Constants;
using NHIGIA.Common.Enums;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;
using NHIGIA.Repository.Repositories;
using NHIGIA.Services.Interfaces;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;

namespace NHIGIA.Services.Implementations
{
    public class EmployeesOnBusinessTripAssignedStaffService : IEmployeesOnBusinessTripAssignedStaffService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly EmployeesOnBusinessTripAssignedStaffRepository _repository = new EmployeesOnBusinessTripAssignedStaffRepository();

        public ResponseList<EmployeesOnBusinessTripAssignedStaffViewModel> GetAllEmployeesOnBusinessTripAssignedStaff(int employeesOnBusinessTripId)
        {
            _logger.Trace("Start EmployeesOnBusinessTripAssignedStaffService - GetAllEmployeesOnBusinessTripAssignedStaff: " + DateTime.Now);
            var result = _repository.GetAllEmployeesOnBusinessTripAssignedStaff(employeesOnBusinessTripId);
            _logger.Info("EmployeesOnBusinessTripAssignedStaffService - GetAllEmployeesOnBusinessTripAssignedStaff - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End EmployeesOnBusinessTripAssignedStaffService - GetAllEmployeesOnBusinessTripAssignedStaff: " + DateTime.Now);
            return new ResponseList<EmployeesOnBusinessTripAssignedStaffViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public string SaveEmployeesOnBusinessTripAssignedStaff(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start EmployeesOnBusinessTripAssignedStaffService - SaveEmployeesOnBusinessTripAssignedStaff: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<EmployeesOnBusinessTripAssignedStaffViewModel>(content);
                var listData = new List<EmployeesOnBusinessTripAssignedStaffEntity>();
                // for action add
                if (saveData.ListEmployeeId != null && saveData.ListEmployeeId.Count > 0)
                {
                    foreach (var item in saveData.ListEmployeeId)
                    {
                        var listItem = new EmployeesOnBusinessTripAssignedStaffEntity()
                        {
                            EmployeesOnBusinessTripId = item.EmployeesOnBusinessTripId,
                            EmployeeId = item.EmployeeId
                        };
                        listData.Add(listItem);
                    }
                    var data = listData.ConvertToCustomUserDefinedDataTable();
                    var result = _repository.SaveEmployeesOnBusinessTripAssignedStaff(data, isAction);
                    if (result != null)
                    {
                        _logger.Info("EmployeesOnBusinessTripAssignedStaffService - SaveEmployeesOnBusinessTripAssignedStaff - Data: " + JsonConvert.SerializeObject(result));
                        _logger.Trace("End EmployeesOnBusinessTripAssignedStaffService - SaveEmployeesOnBusinessTripAssignedStaff: " + DateTime.Now);
                        return JsonConvert.SerializeObject(Constants.MessageInformation.Success);
                    }
                    _logger.Info("EmployeesOnBusinessTripAssignedStaffService - SaveEmployeesOnBusinessTripAssignedStaff - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End EmployeesOnBusinessTripAssignedStaffService - SaveEmployeesOnBusinessTripAssignedStaff: " + DateTime.Now);
                }
                else
                {
                    // for action delete
                    if (isAction == (int)ActionCode.Delete)
                    {
                        var listItem = new EmployeesOnBusinessTripAssignedStaffEntity()
                        {
                            Id = saveData.Id
                        };
                        listData.Add(listItem);
                        var data = listData.ConvertToCustomUserDefinedDataTable();
                        var result = _repository.SaveEmployeesOnBusinessTripAssignedStaff(data, isAction);
                        if (result != null)
                        {
                            _logger.Info("EmployeesOnBusinessTripAssignedStaffService - SaveEmployeesOnBusinessTripAssignedStaff - Data: " + JsonConvert.SerializeObject(result));
                            _logger.Trace("End EmployeesOnBusinessTripAssignedStaffService - SaveEmployeesOnBusinessTripAssignedStaff: " + DateTime.Now);
                            return JsonConvert.SerializeObject(Constants.MessageInformation.Success);
                        }
                    }
                }
                return JsonConvert.SerializeObject(new EmployeesOnBusinessTripAssignedStaffEntity());
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return JsonConvert.SerializeObject(Constants.MessageInformation.Failed);
            }
        }
    }
}
