using WMSBrokerProject.Models;

namespace WMSBrokerProject.Interfaces
{
    public interface IGoEfficientService
    {
        Task<ResponseModel<RES4Model>> REQ4_GetProIDAsync(REQ4Model model);                          //Request 04 for getting PROCID
        Task<ResponseModel<RES4aModel>> REQ4a_GetTemplateFromGoEfficient(REQ4aModel model);           //Request 04a for getting Template
        Task<ResponseModel<RES5Model>> REQ5_SaveRecordToGoEfficient(REQ5Model model);                //Request 05 for saving the information
        Task<ResponseModel<RES5aModel>> REQ5a_SaveAddressToGoEfficient(REQ5aModel model);             //Request 05a for saving address
        Task<ResponseModel<RES5bModel>> REQ5b_AddFilesToGoEfficient(REQ5bModel model);
        Task<ResponseModel<RES6Model>> REQ6_IsRecordExist(REQ6Model model);                          //Request 06 for checking if record exists 
        Task<ResponseModel<Dictionary<string, string>>> GetGoEfficientAttributes();             //to get attributes dynamically from json for goefficient
        Task<ResponseModel<Dictionary<string, string>>> GetGoEfficientFileAttributes();
        Task<ResponseModel<RES4aTemplate>> FillDataIn4aTemplate(RES4aTemplate template, TaskFetchResponse2Model model);
        Task<ResponseModel<string>> GetKeyForValueInRES3aMapping(string value);
        Task<ResponseModel<string>> GetKeyForRES4Mapping();
        Task<ResponseModel<RES4aTemplate>> FillDataIn4aAddressTemplate(RES4aTemplate template, TaskFetchResponse2Model model);
        Task<ResponseModel<Dictionary<string, object>>> FillDataInBeheerderAttributesDictionary(TaskFetchResponseModel model);

	}
}
