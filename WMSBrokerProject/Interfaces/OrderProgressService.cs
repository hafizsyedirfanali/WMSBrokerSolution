using WMSBrokerProject.Models;

namespace WMSBrokerProject.Interfaces
{
    public interface IOrderProgressService
    {
        Task<ResponseModel<OrderProcessingTemplateResponse>> GetTemplateIds();
        
        Task<ResponseModel<RES7Model>> REQ7GetTaskIDs(REQ7Model model);//GoEfficient_Request07_ReadOpenTasksByTemplateID_REQ7
        Task<ResponseModel<TTRES4aModel>> REQ4a_GetTemplateFromGoEfficient(OrderProcessingREQ4aModel model);//GoEfficient_InstantiatedAttacmentsRequest_REQ04a
        Task<ResponseModel<UIARES5Model>> REQ05_UpdateInstantiatedAttachmentsRequest(UIAREQ5Model model);//GoEfficient_UpdateInstantiatedAttachmentsRequest_REQ05
        Task<ResponseModel<TaskIndicationResponseModel>> RequestTaskIndication(TaskIndicationRequestModel model);
       // Task<ResponseModel<TTRES4Model>> REQ4_TrackAndTrace(TTREQ4Model model);
        Task<ResponseModel<CTRES7aModel>> REQ7a(CTREQ7aModel model);
    }
}
