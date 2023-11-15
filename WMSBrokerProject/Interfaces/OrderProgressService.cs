using WMSBrokerProject.Models;

namespace WMSBrokerProject.Interfaces
{
    public interface IOrderProgressService
    {
        Task<ResponseModel<TrackTraceTemplateResponse>> GetTemplateIds();
        Task<ResponseModel<RES7Model>> REQ7GetTaskIDs(REQ7Model model);
        Task<ResponseModel<TTRES4aModel>> REQ4a_GetTemplateFromGoEfficient(TTREQ4aModel model);
        Task<ResponseModel<TTRES5Model>> REQ05_UpdateInstantiatedAttachmentsRequest(TTREQ5Model model);
        Task<ResponseModel<TaskIndicationResponseModel>> RequestTaskIndication(TaskIndicationRequestModel model);
       // Task<ResponseModel<TTRES4Model>> REQ4_TrackAndTrace(TTREQ4Model model);
        Task<ResponseModel<TTRES7aModel>> REQ7a(TTREQ7aModel model);
    }
}
