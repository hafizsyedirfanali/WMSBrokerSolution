using Newtonsoft.Json.Linq;
using WMSBrokerProject.Models;

namespace WMSBrokerProject.Interfaces
{
    public interface IGoEfficientService
    {
        Task<ResponseModel<RES4Model>> REQ4_GetProIDAsync(REQ4Model model);                          //Request 04 for getting PROCID
        Task<ResponseModel<RES4aModel>> REQ4a_GetTemplateFromGoEfficient(REQ4aModel model);           //Request 04a for getting Template
        Task<ResponseModel<RES5Model>> REQ5_SaveRecordToGoEfficient(REQ5Model model);                //Request 05 for saving the information
        Task<ResponseModel<RES5aModel>> REQ5a_SaveAddressToGoEfficient(REQ5aModel model);             //Request 05a for saving address
        Task<ResponseModel<RES6Model>> REQ6_IsRecordExist(REQ6Model model);                          //Request 06 for checking if record exists 
        Task<ResponseModel<CTRES7aModel>> REQ7a(CTREQ7aModel model);//GoEfficient_Request07aCloseTask_REQ7a
        Task<ResponseModel<Dictionary<string, string>>> GetGoEfficientAttributes();             //to get attributes dynamically from json for goefficient
        Task<ResponseModel<Dictionary<string, string>>> GetGoEfficientFileAttributes();
        Task<ResponseModel<RES4aTemplate>> FillDataIn4aTemplate(RES4aTemplate template, TaskFetchResponse2Model model);
        Task<ResponseModel<RES4aTemplate>> FillFCDataIn4aTemplate(RES4aModel res4aModel, TaskFetchResponse2Model model);
        Task<ResponseModel<string>> GetKeyForValueInRES3aMapping(string value);
        Task<ResponseModel<Dictionary<string, object?>>> GetAttributeValueDictionaryByAction(string action, JObject taskFetchJsonObject);
        Task<ResponseModel<string>> GetKeyForRES4Mapping();
        Task<ResponseModel<RES4aTemplate>> FillDataIn4aAddressTemplate(RES4aTemplate template, TaskFetchResponse2Model model);
        Task<ResponseModel<Dictionary<string, object>>> FillSourcePathInBeheerderAttributesDictionary(TaskFetchResponse model);
        Task<ResponseModel<Dictionary<string, object>>> FillDataInBeheerderAttributesDictionary(TaskFetchResponse model, Dictionary<string, object> sourcePathInBeheerderAttributesDictionary);
        Task<ResponseModel<REQ4Model>> FillDataForRequest4(Dictionary<string, object> dataDictionary);
        Task<ResponseModel<string?>> GetWMSBeheerderRES4AddressMappingValue(string addressKeyName);
        Task<ResponseModel<Dictionary<string, string>>> GetKeyValuesFromWMSBeheerderAddresses(string addressKeyName);
    }
}
