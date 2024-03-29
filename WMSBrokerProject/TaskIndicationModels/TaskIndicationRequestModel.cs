﻿using System.ComponentModel.DataAnnotations;

namespace WMSBrokerProject.TaskIndicationModels;

public class TaskIndicationRequestModel
{
    public TaskIndicationRequestModel()
    {
        header = new Header();
    }
    public Header? header { get; set; }
    public string taskId { get; set; }
    
}
public class Header
{
    public From from { get; set; }
    public int updateCount { get; set; }
    public DateTime created { get; set; }
    public string priority { get; set; }
}
public class From
{
    public string orgId { get; set; }
    public string systemId { get; set; }
}