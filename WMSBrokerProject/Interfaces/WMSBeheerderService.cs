using WMSBrokerProject.Models;

namespace WMSBrokerProject.Interfaces
{
	public interface IWMSBeheerderService
	{
		Task<ResponseModel<TaskFetchResponseModel>> Request2TaskFetch(REQ2Model model);
		Task<ResponseModel<TaskSyncResponseModel>> RequestTaskSync(TaskSyncRequestModel model);
	}
}
