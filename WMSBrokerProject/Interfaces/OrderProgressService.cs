using Newtonsoft.Json.Linq;
using WMSBrokerProject.Models;

namespace WMSBrokerProject.Interfaces
{
    public interface IOrderProgressService
    {
        Task<ResponseModel<OrderProcessingTemplateResponse>> GetTemplateIds();
        
        Task<ResponseModel<RES7Model>> REQ7GetPro_IDs(REQ7Model model);//GoEfficient_Request07_ReadOpenTasksByTemplateID_REQ7
        Task<ResponseModel<OPRES4aModel>> REQ4a_GetInID(OrderProcessingREQ4aModel model);
        Task<ResponseModel<Res4aGetTemplateModel>> REQ4a_GetTemplateData(REQ4aGetTemplateModel model);
        Task<ResponseModel<UIARES5Model>> REQ05_UpdateInstantiatedAttachmentsRequest(UIAREQ5Model model);//GoEfficient_UpdateInstantiatedAttachmentsRequest_REQ05
        Task<ResponseModel<TaskIndicationResponseModel>> RequestTaskIndication(TaskIndicationRequestModel model);
        Task<ResponseModel<CTRES7aModel>> REQ7a(CTREQ7aModel model);
        Task<ResponseModel<JObject>> GetJsonResultForTaskFetchResponse(Res4aGetTemplateModel templateModel, string actionName);


    }
}
