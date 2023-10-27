/*
 * DFN WMS Active-Operator interface description
 *
 * API of the WMS for use by an active operator (level2) who wants to use the WMS task functionality
 *
 * OpenAPI spec version: 1.1.3
 * Contact: support@infodation.nl
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace WMSBrokerProject.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    public class TaskFetchApiController : ControllerBase
    {
        /// <summary>
        /// Task Fetch
        /// </summary>
        /// <remarks>get full Active-Operator oriented task databased on InId from AO</remarks>
        /// <param name="orgId">registered code for sender of the message (AO or passive operator)</param>
        /// <param name="inId">ID in the source system of AO. used for doing updates</param>
        /// <param name="xRequestID"> unique X-Request-Id header which is used to ensure idempotent message processing in case of a retry</param>
        /// <param name="xCorrelationID">Correlates HTTP requests between a client and server. Should be same value in TaskIndication</param>
        /// <param name="xWMSTest">when value is true, this message is part of a test. Process the message as normal, same business logic. in the end, don&#x27;t actually physically execute the order. This order is not supposed to have a material impact. No customer impacts is allowed to happen because of this task. This flag can trigger extra logging at all involved systems during processing.  When other messages are sent because of a message with the this test flag set, those other messages *must* also include a test flag. </param>
        /// <param name="xWMSAPIVersion">request response in given API version</param>
        /// <response code="200">Success</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Missing required credentials</response>
        /// <response code="403">Forbidden because these credentials do not give the needed access rights</response>
        /// <response code="404">connection not found</response>
        /// <response code="429">too many requests too fast</response>
        /// <response code="500">Internal server error, dont try this request again</response>
        /// <response code="503">Temporary failure in service , try again later</response>
        [HttpGet]
        [Route("/ActiveOperator/{orgId}/tasks/{inId}")]
        [Authorize(AuthenticationSchemes = BearerAuthenticationHandler.SchemeName)]
        [ValidateModelState]
        [SwaggerOperation("TaskFetch")]
        [SwaggerResponse(statusCode: 200, type: typeof(TaskType), description: "Success")]
        [SwaggerResponse(statusCode: 400, type: typeof(string), description: "Bad Request")]
        public virtual IActionResult TaskFetch([FromRoute][Required][StringLength(15, MinimumLength = 3)] string orgId, [FromRoute][Required][StringLength(36, MinimumLength = 1)] string inId, [FromHeader][StringLength(36, MinimumLength = 1)] string xRequestID, [FromHeader][StringLength(36, MinimumLength = 1)] string xCorrelationID, [FromHeader] bool? xWMSTest, [FromHeader][StringLength(8, MinimumLength = 1)] string xWMSAPIVersion)
        {
            #region Circet_Implementation
            string strFinLogMemo = "", strError = "";
            string strAftAction = "";
            int intFinIdLog = 0, intProId = 0;
            KeyedArray objContext = new KeyedArray();
            Helper.SetContext(objContext);
            //Go Data ophalen

            try
            {
                Helper.UpdateInterfaceLog(objContext, "AO-Circet", "TF_REQUEST", inId, orgId, "");
                if (int.TryParse(inId, out intProId) == false)
                {
                    strError = "inId: " + inId + " is not a valid PRO ID";
                    Helper.UpdateInterfaceLog(objContext, "AO-Circet", "TF_REQUEST_ERROR", inId, orgId, "StatusCode 204: " + strError);
                    return StatusCode(204);
                }

                Stub objStub = new Stub();
                KeyedArray objConditions = new KeyedArray(), objFields = new KeyedArray(), objFins = new KeyedArray(), objTemp;
                KeyedGrid objGrid;
                string strAanhef = "";
                int i;

                objFields.Add("FIN.FIN_NAME");          //0        
                objFields.Add("FIN.FIN_ID");            //1
                objFields.Add("FIN.FIN_PATH");          //2
                objFields.Add("FIN.FIN_RECORD_ID");     //3
                objFields.Add("FIN.FIN_DATE");          //4
                objFields.Add("FIN.FIN_NUMBER");        //5
                objFields.Add("FIN.FIN_MEMO");          //6
                objFields.Add("FIN.FIN_FILE_EXT");      //7
                objFields.Add("FIN.FIN_ADRESS_ID");     //8
                objFields.Add("UDF.UDF_TYPE");          //9
                objFields.Add("UDF.UDF_TYPEINFO");      //10
                objFields.Add("UDF.UDF_LABEL");         //11
                objFields.Add("PRO.PRO_DESCRIPTION");   //12
                objFields.Add("FIN.FIN_NAME_L");        //13
                objFields.Add("PRO.PRO_CREATED");       //14
                objFields.Add("FIN.FIN_CREATED");       //15
                objFields.Add("FIN.FIN_CHANGED");       //16
                objConditions.Add(inId, "PRO.PRO_ID");   //9240768

                objGrid = objStub.Read(objContext, "FIN_PRO_READ", new SQLCondition(objConditions), objFields, "");

                TaskType t = new TaskType();

                if (objGrid.Count > 0)
                {
                    for (i = 0; i < objGrid.Count; i++)
                    {
                        switch (objGrid[i, "FIN.FIN_NAME_L"])
                        {
                            case "cifwms-updatecount":
                                objFins.Add(objGrid[i, "FIN.FIN_NUMBER"], objGrid[i, "FIN.FIN_NAME_L"].ToString());
                                break;
                            case "type product":
                                objFins.Add(objGrid[i, "FIN.FIN_PATH"], objGrid[i, "FIN.FIN_NAME_L"].ToString());
                                break;
                            case Helper.CIFWMS_AFT_STATUS:
                                objFins.Add(objGrid[i, "FIN.FIN_PATH"], objGrid[i, "FIN.FIN_NAME_L"].ToString());
                                break;
                            case Helper.CIFWMS_AFT_SUBSTATUS:
                                objFins.Add(objGrid[i, "FIN.FIN_PATH"], objGrid[i, "FIN.FIN_NAME_L"].ToString());
                                break;
                            case Helper.CIFWMS_AFT_ACTION:
                                objFins.Add(objGrid[i, "FIN.FIN_PATH"], objGrid[i, "FIN.FIN_NAME_L"].ToString());
                                break;
                            case "aee_frame":
                            case "aee_block":
                            case "aee_module":
                            case "aee_port":
                            case "aee_connector":
                            case "naam-aanvrager":
                            case "mobiel aanvrager":
                            case "e-mail aanvrager":
                            case "cifwms-dhid":
                            case "cifwms-reason":
                                objFins.Add(objGrid[i, "FIN.FIN_PATH"], objGrid[i, "FIN.FIN_NAME_L"].ToString());
                                break;
                            case "cifwms-clarification":
                                objFins.Add(objGrid[i, "FIN.FIN_PATH"], objGrid[i, "FIN.FIN_NAME_L"].ToString());
                                break;
                            case "cifwms-plannedstarttime":
                            case "cifwms-plannedstartday":
                            case "cifwms-plannedendtime":
                            case "cifwms-plannedendday":
                                objFins.Add(objGrid[i, "FIN.FIN_DATE"], objGrid[i, "FIN.FIN_NAME_L"].ToString());
                                break;
                            case "cifwms-fibernumber":
                                objFins.Add(objGrid[i, "FIN.FIN_PATH"], objGrid[i, "FIN.FIN_NAME_L"].ToString());
                                break;
                            case "cifwms-areanetworktype":
                                objFins.Add(objGrid[i, "FIN.FIN_PATH"], objGrid[i, "FIN.FIN_NAME_L"].ToString());
                                break;
                            case "plattegrond":
                                break;
                            case "opmerkingen":
                                //DateTime proCreated = (DateTime)objGrid[i, "PRO.PRO_CREATED"];//This is not the date of the comments is created
                                DateTime FINCreated = DateTime.UtcNow;
                                if (!(objGrid[i, "FIN.FIN_CREATED"] == System.DBNull.Value))
                                {
                                    FINCreated = ((DateTime)objGrid[i, "FIN.FIN_CREATED"]).ToUniversalTime();
                                    if (!(objGrid[i, "FIN.FIN_CHANGED"] == System.DBNull.Value))
                                    {
                                        FINCreated = Helper.GetMaxDate(FINCreated, ((DateTime)objGrid[i, "FIN.FIN_CHANGED"]).ToUniversalTime());
                                    }
                                };
                                ///'fincreated= 
                                if (objGrid[i, "FIN.FIN_PATH"].ToString().Length > 0)
                                {
                                    {
                                        Comment comment = new Comment
                                        {
                                            Party = new Party
                                            {
                                                OrgId = objContext.GetValue("OrgId_Source", "").ToString(),
                                                SystemId = objContext.GetValue("SystemId_Source", "").ToString()
                                            },
                                            Id = Guid.NewGuid(),
                                            //TODO: discuss on which Author to use and whether to only push the comments with TaskFetch once at conception
                                            //TODO: split comments from CIF WMS in the same way as done already for the SOAP interface
                                            Author = new Author()
                                            {
                                                FirstName = "circet",
                                                LastName = "go"
                                            },
                                            Note = objGrid[i, "FIN.FIN_PATH"].ToString(),
                                            //Created = proCreated.ToUniversalTime(),
                                            Created = FINCreated.ToUniversalTime(),
                                        };
                                        t.Comments = new List<Comment>();
                                        t.Comments.Add(comment);
                                    }
                                }
                                break;
                            case "wensdatum aansluiting":
                                if (objGrid[i, "FIN.FIN_DATE"].ToString().Length > 0)

                                {
                                    DateTime proWishDate = (DateTime)objGrid[i, "FIN.FIN_DATE"];
                                    t.WishDate = new TaskTypeWishDate();
                                    t.WishDate.WishStart = proWishDate.ToUniversalTime();
                                }
                                break;
                            case "technische offerte":
                                objFins.Add(objGrid[i, "FIN.FIN_PATH"], objGrid[i, "FIN.FIN_NAME_L"].ToString());
                                //t.TaskInfo.HasInfo.Payment.ReferenceContractor = (string)objGrid[i, "FIN.FIN_PATH"];
                                break;
                            case "bedrag technische offerte":
                                objFins.Add(objGrid[i, "FIN.FIN_NUMBER"], objGrid[i, "FIN.FIN_NAME_L"].ToString());
                                break;
                            case "situatieschets":
                                objTemp = new KeyedArray();
                                objTemp.Add("", "id");
                                objTemp.Add("", "name");
                                objTemp.Add("Circet Veenendaal", "author");
                                objTemp.Add("", "path");
                                objFins.AddIfNotExists(objTemp, objGrid[i, "FIN.FIN_NAME_L"].ToString());
                                break;
                            case "aanleg-adres":
                                objFins.Add(objGrid[i, "FIN.FIN_ADRESS_ID"], objGrid[i, "FIN.FIN_NAME_L"].ToString());
                                break;
                            case "kamer":
                                objFins.Add(objGrid[i, "FIN.FIN_PATH"], objGrid[i, "FIN.FIN_NAME_L"].ToString());
                                break;
                            case "cifwms-logbook":
                                intFinIdLog = (int)objGrid[i, "FIN.FIN_ID"];
                                if ((string)Helper.GetFieldValue(objGrid[i, "FIN.FIN_MEMO"], "") != "") { strFinLogMemo = (string)Helper.GetFieldValue(objGrid[i, "FIN.FIN_MEMO"], ""); }
                                break;
                            case "aanhef-aanvrager":
                                // objAanhef.Add(o.Values[10], "udf");
                                strAanhef = Helper.GetUdfTitle(objGrid[i, "FIN.FIN_PATH"].ToString());
                                //          Dim objUDf As KeyedArray = DisplayFormatToFixedCodesBoaDuplicate("", objRead.Rows(0)(1))
                                //For Each objFCD As FixedCode In objUDf
                                //  If objFCD.Code = objKlant.ToString Then
                                //    strFcd = objFCD.Name : Exit For
                                //  End If            
                                //Next            
                                objFins.Add(strAanhef, objGrid[i, "FIN.FIN_NAME_L"].ToString());
                                break;
                            case "cifwms-plannedstartperiod":
                                break;
                            case "cifwms-plannedendperiod":
                                objFins.Add(objGrid[i, "FIN.FIN_PATH"], objGrid[i, "FIN.FIN_NAME_L"].ToString());
                                break;
                            case "waar zit nu de aansluiting?":
                                objFins.Add(objGrid[i, "FIN.FIN_PATH"], objGrid[i, "FIN.FIN_NAME_L"].ToString());
                                break;
                            case "waar wilt u de aansluiting?":
                                objFins.Add(objGrid[i, "FIN.FIN_PATH"], objGrid[i, "FIN.FIN_NAME_L"].ToString());
                                break;
                            default:
                                String tt = objGrid[i, "FIN.FIN_NAME_L"].ToString();
                                break;
                        }
                    }
                    strFinLogMemo = DateTime.Now.ToString() + " TaskFetch received from CIFWMS" + Environment.NewLine + strFinLogMemo;
                    Helper.UpdateFinLog(objContext, intFinIdLog, strFinLogMemo.Substring(0, Math.Min(strFinLogMemo.Length, 4000)));

                    t.InId = inId.ToString();
                    t.TaskInfo = new TaskInfo();

                    if ((objFins.GetValue("cifwms-updatecount", null) == null))
                    {
                        Helper.UpdateInterfaceLog(objContext, "AO-Circet", "TF_ERROR", inId, orgId, "StatusCode(204) OR UpdateCount Fin does not exist");
                        return StatusCode(204);
                    }

                    decimal decUpdateCount = 0;
                    if ((objFins.GetIndex("cifwms-updatecount") > -1))
                    {
                        if (objFins.GetValue("cifwms-updatecount", null) is System.DBNull)
                        {
                            Helper.UpdateInterfaceLog(objContext, "AO-Circet", "TF_ERROR", inId, orgId, "StatusCode(204) OR UpdateCount Fin is System.DBNull, assume 0.00.");
                            objFins.SetValue("cifwms-updatecount", decUpdateCount);
                        }
                    }

                    string strNetworkType = objFins.GetValue("cifwms-areanetworktype".ToLower(), "").ToString();
                    strAftAction = objFins.GetValue("cifwms-aft-action".ToLower(), "").ToString();
                    if (strNetworkType == "" || strAftAction == "")
                    {
                        Config.Mapping mapped;
                        mapped = Helper.GetNetworkTypeByProduct((string)objFins.GetValue("type product", ""));
                        if (mapped == null)
                        {
                            if (strNetworkType == "")
                            {
                                strNetworkType = ((int)TaskTypeArea.NetworkTypeEnum.FIBEREnum).ToString();
                            }
                            if (strAftAction == "")
                            {
                                strAftAction = ""; //Leave AftAction empty
                            }
                        }
                        else
                        {
                            if (strNetworkType == "")
                            {
                                strNetworkType = mapped.NetworkType;
                            }
                            if (strAftAction == "")
                            {
                                strAftAction = mapped.Aft_Action;
                                objFins.SetValue(Helper.CIFWMS_AFT_ACTION, strAftAction);
                            }
                        }
                    };

                    ////t.Header = Helper.SetHeader((DateTime)objGrid[0, "PRO.PRO_CREATED"], (int)System.Math.Floor((decimal)objFins.GetValue("cifwms-updatecount", 0)), objContext);
                    t.Header = Helper.SetHeader(DateTime.Now, (int)System.Math.Floor((decimal)objFins.GetValue("cifwms-updatecount", 0)), objContext);
                    bool blnHasInfo = false; bool blnOutletStatus = false; bool blnTargetOutletStatus = false; bool blnQuote = false; bool blnConnectionAddress = false;
                    switch (Helper.GetActionType(objFins.GetValue(Helper.CIFWMS_AFT_ACTION, strAftAction).ToString()))
                    {
                        case ActionType.NEWCONNECTIONEnum:
                            {
                                blnConnectionAddress = true;
                                blnHasInfo = true;
                                blnQuote = true;
                                break;
                            }
                        case ActionType.AFTERCONNECTEnum:
                            {
                                blnConnectionAddress = true;
                                blnHasInfo = true;
                                blnQuote = true;
                                break;
                            }
                        case ActionType.FTUREPLACEMENTEnum:
                            {
                                blnConnectionAddress = true;
                                blnHasInfo = true;
                                blnOutletStatus = true;
                                blnTargetOutletStatus = true;
                                blnQuote = true;
                                break;
                            }
                        case ActionType.FTUDISPLACEMENTEnum:
                            {
                                blnConnectionAddress = true;
                                blnHasInfo = true;
                                blnOutletStatus = true;
                                blnQuote = true;
                                break;
                            }
                        default:
                            Helper.UpdateInterfaceLog(objContext, "AO-Circet", "TF_REQUEST_ERROR", inId, orgId, "StatusCode 500: " + "ActionType " + Helper.GetActionType(objFins.GetValue(Helper.CIFWMS_AFT_ACTION, "").ToString()) + " not supported.");
                            return StatusCode(500);
                    }
                    HASInfo objHASInfo = null;
                    if (blnHasInfo)
                    {
                        objHASInfo = new HASInfo();
                        if ((string)Helper.GetFieldValue(objFins.GetValue("cifwms-dhid", ""), "").ToString() != "")
                        {
                            objHASInfo.DHid = (string)objFins.GetValue("cifwms-dhid", "");

                        }
                        string strFiberNumber = objFins.GetValue("cifwms-fibernumber".ToLower(), FiberNumber.NUMBER_1).ToString();
                        objHASInfo.FiberNumber = EnumHelper.GetEnumByNumber<FiberNumber>(strFiberNumber);
                    }
                    if (blnOutletStatus)
                    {
                        if ((string)Helper.GetFieldValue(objFins.GetValue("Waar zit nu de aansluiting?".ToLower(), ""), "").ToString() != "")
                        {
                            string strOutletStatus = objFins.GetValue("Waar zit nu de aansluiting?".ToLower(), "").ToString();
                            objHASInfo.OutletStatus = Helper.GetUdfTextByCode(objContext, "CIFWMS-OutletStatus", strOutletStatus);
                        }
                    }

                    if (blnTargetOutletStatus)
                    {
                        if ((string)Helper.GetFieldValue(objFins.GetValue("Waar wilt u de aansluiting?".ToLower(), ""), "").ToString() != "")
                        {
                            string strTargetOutletStatus = objFins.GetValue("Waar wilt u de aansluiting?".ToLower(), "").ToString();
                            objHASInfo.TargetOutletStatus = Helper.GetUdfTextByCode(objContext, "CIFWMS-OutletStatus", strTargetOutletStatus);
                        }
                    }
                    if (blnConnectionAddress)
                    {
                        if (objFins.GetValue("aanleg-adres", null) != null)
                        {
                            KeyedGrid objAddressGrid = new KeyedGrid();
                            KeyedArray objGridFields = new KeyedArray(), objGridConditions = new KeyedArray();
                            objGridFields.Add("ADRESS.ADRESS_CNTR_ISO3166A3");
                            objGridFields.Add("ADRESS.ADRESS_TOWN");
                            objGridFields.Add("ADRESS.ADRESS_STREET");
                            objGridFields.Add("ADRESS.ADRESS_ZIPCODE");
                            objGridFields.Add("ADRESS.ADRESS_HOUSNR");
                            objGridFields.Add("ADRESS.ADRESS_HOUSNR_SFX");

                            objGridConditions.Add(objFins.GetValue("aanleg-adres", ""), "ADRESS.ADRESS_ID");
                            objAddressGrid = objStub.Read(objContext, "ADRESS_READ_M_V1", objGridConditions, objGridFields, "");

                            if (objAddressGrid.Count > 0)
                            {
                                // string strFiberNumber = objFins.GetValue("cifwms-fibernumber".ToLower(), FiberNumber.NUMBER_1).ToString();
                                // objHASInfo.FiberNumber = EnumHelper.GetEnumByNumber<FiberNumber>(strFiberNumber);
                                // 

                                t.Area = new TaskTypeArea()
                                {
                                    //TODO: Make FIBER value dynamic based on input from GoEfficient order information
                                    NetworkType = EnumHelper.GetEnumByNumber<TaskTypeArea.NetworkTypeEnum>(strNetworkType)
                                };
                                //TODO: What to do with Housenumber ++++
                                NLDAddress objAddress = new NLDAddress();
                                objAddress.PostalCode = (string)Helper.GetFieldValue(objAddressGrid[0, "ADRESS.ADRESS_ZIPCODE"], "").ToString().Trim();    //mandatory
                                objAddress.HouseNumber = Convert.ToInt32(Helper.GetFieldValue(objAddressGrid[0, "ADRESS.ADRESS_HOUSNR"], 0).ToString().Trim());     //mandatory

                                if (objAddress.PostalCode != "" && objAddress.HouseNumber != 0)
                                {
                                    if ((string)Helper.GetFieldValue(objAddressGrid[0, "ADRESS.ADRESS_CNTR_ISO3166A3"], "") != "") { objAddress.Country = (string)Helper.GetFieldValue(objAddressGrid[0, "ADRESS.ADRESS_CNTR_ISO3166A3"], "").ToString().Trim(); }
                                    //TODO: Support for Room to be implemented? (For now not possible to have a room indication)
                                    if ((string)Helper.GetFieldValue(objAddressGrid[0, "ADRESS.ADRESS_HOUSNR_SFX"], "") != "") { objAddress.HouseNumberExtension = (string)Helper.GetFieldValue(objAddressGrid[0, "ADRESS.ADRESS_HOUSNR_SFX"], "").ToString().Trim(); }
                                    if ((string)Helper.GetFieldValue(objAddressGrid[0, "ADRESS.ADRESS_STREET"], "") != "") { objAddress.StreetName = (string)Helper.GetFieldValue(objAddressGrid[0, "ADRESS.ADRESS_STREET"], "").ToString().Trim(); }
                                    if ((string)Helper.GetFieldValue(objAddressGrid[0, "ADRESS.ADRESS_TOWN"], "") != "") { objAddress.City = (string)Helper.GetFieldValue(objAddressGrid[0, "ADRESS.ADRESS_TOWN"], "").ToString().Trim(); }
                                    if (objFins.GetValue("kamer", "").ToString().Length > 0) { objAddress.Room = objFins.GetValue("kamer", "").ToString(); }
                                    objHASInfo.ConnectionAddress = objAddress;
                                }
                            }
                        }

                        else
                        {
                            Helper.UpdateInterfaceLog(objContext, "AO-Circet", "TF_REQUEST_ERROR", inId, orgId, "StatusCode 204: " + "Address FIN_NAME 'aanleg - adres' not found");
                            return StatusCode(204);
                        }
                    }

                    ContactPerson objContactPerson = new ContactPerson
                    {
                        LastName = (string)Helper.GetFieldValue(objFins.GetValue("naam-aanvrager", ""), "<onbekend>"),     //mandatory
                        PhoneNumber = (string)Helper.GetFieldValue(objFins.GetValue("mobiel aanvrager", ""), "0000000000"),  //mandatory
                    };

                    if (objFins.GetValue("e-mail aanvrager", System.DBNull.Value) != System.DBNull.Value)
                    { objContactPerson.Email = (string)Helper.GetFieldValue(objFins.GetValue("e-mail aanvrager", ""), ""); }
                    if ((string)objFins.GetValue("aanhef-aanvrager", "").ToString() != "")
                    {
                        //Removed due too "bug" on inconsistency WMS Portal Create & Update Contact Info => Infodation removed Title
                        //objContactPerson.Title = (string)Helper.GetFieldValue(objFins.GetValue("aanhef-aanvrager", ""), ""); 
                    }
                    if (blnQuote)
                    {
                        #region Circet_Payment
                        if (!(objFins.GetValue("technische offerte", System.DBNull.Value) == System.DBNull.Value && objFins.GetValue("bedrag technische offerte", System.DBNull.Value) == System.DBNull.Value))
                        {
                            Double dblAmount;
                            Double.TryParse(objFins.GetValue("bedrag technische offerte", 0.00).ToString(), out dblAmount);
                            //Reference contractor mag niet langer dan 100 char.  zijn. Gaat dan alsnog fout bij het uploaden attachments.
                            string strReferenceContractorTemp = objFins.GetValue("technische offerte", "").ToString();
                            objHASInfo.Payment = new Payment();
                            if (!(objFins.GetValue("bedrag technische offerte", System.DBNull.Value) == System.DBNull.Value))
                            {
                                objHASInfo.Payment.Amount = dblAmount;
                            }
                            if (!(objFins.GetValue("technische offerte", System.DBNull.Value) == System.DBNull.Value))
                            {
                                objHASInfo.Payment.ReferenceContractor = strReferenceContractorTemp;
                            }
                            #endregion Circet_Payment
                        };

                    }
                    if (blnHasInfo)
                    {
                        objHASInfo.ContactPerson = objContactPerson;
                        t.TaskInfo.HasInfo = objHASInfo;
                    }
                    t.Action = EnumHelper.GetEnumByNumber<ActionType>(objFins.GetValue(Helper.CIFWMS_AFT_ACTION, "").ToString());
                    //t.Action = Helper.GetActionType(objFins.GetValue(Helper.CIFWMS_AFT_ACTION, "").ToString());

                    TaskStatus objTaskStatus = new TaskStatus();
                    objTaskStatus.MainStatus = Helper.GetMainStatus(objFins.GetValue(Helper.CIFWMS_AFT_STATUS, "").ToString());
                    if (objFins.GetValue(Helper.CIFWMS_AFT_SUBSTATUS, "").ToString() != "")
                    {
                        objTaskStatus.SubStatus = EnumHelper.GetEnumByNumber<TaskStatus.SubStatusEnum>(objFins.GetValue(Helper.CIFWMS_AFT_SUBSTATUS, "").ToString());
                    }
                    //20221221 Do not sent cifwms-reason and cifwms-clarification to keep a clean order.
                    //Maybe IF mainstatus=Cancelled then adding Reason & Clarification should be added.
                    //if ((string)objFins.GetValue("cifwms-clarification", "").ToString() != "") { objTaskStatus.Clarification = (string)objFins.GetValue("cifwms-clarification", ""); }
                    //if ((string)objFins.GetValue("cifwms-reason", "").ToString() != "") { objTaskStatus.Reason = (string)objFins.GetValue("cifwms-reason", ""); }
                    t.Status = objTaskStatus;

                    if ((string)objFins.GetValue("cifwms-taskid", "").ToString() != "") { t.TaskId = (string)objFins.GetValue("cifwms-taskid", ""); }
                    if ((string)objFins.GetValue("cifwms-relatedtask", "").ToString() != "") { t.RelatedTask = (string)objFins.GetValue("cifwms-relatedtask", ""); }

                    TaskTypePlanned objTaskTypePlanned = new TaskTypePlanned();
                    TaskTypePlannedStart objPlannedStart = new TaskTypePlannedStart();
                    TaskTypePlannedStartDayPeriod objPlannedStartPeriod = new TaskTypePlannedStartDayPeriod();
                    TaskTypePlannedEnd objPlannedEnd = new TaskTypePlannedEnd();
                    TaskTypePlannedEndDayPeriod objPlannedEndPeriod = new TaskTypePlannedEndDayPeriod();

                    if (objFins.GetValue("cifwms-plannedendperiod", "").ToString() != "")
                    {
                        if (objFins.GetValue("cifwms-plannedstartperiod", "").ToString() != "")
                        {
                            //start is optional
                            if (objFins.GetValue("cifwms-plannedstarttime", "").ToString() != "") { objPlannedStart.Time = (DateTime)objFins.GetValue("cifwms-plannedstarttime", ""); }
                            if (objFins.GetValue("cifwms-plannedstartday", "").ToString() != "") { objPlannedStartPeriod.Day = (DateTime)objFins.GetValue("cifwms-plannedstartday", ""); }
                            objPlannedStartPeriod.Period = Helper.GetPeriod((string)objFins.GetValue("cifwms-plannedstartperiod", ""));

                            objPlannedStart.DayPeriod = objPlannedStartPeriod;
                            objTaskTypePlanned.Start = objPlannedStart;
                        }
                        if (objFins.GetValue("cifwms-plannedendtime", "").ToString() != "") { objPlannedEnd.Time = (DateTime)objFins.GetValue("cifwms-plannedendtime", ""); }
                        if (objFins.GetValue("cifwms-plannedendday", "").ToString() != "") { objPlannedEndPeriod.Day = (DateTime)objFins.GetValue("cifwms-plannedendday", ""); }
                        objPlannedEndPeriod.Period = Helper.GetPeriod((string)objFins.GetValue("cifwms-plannedendperiod", ""));
                        objPlannedEnd.DayPeriod = objPlannedEndPeriod;
                        objTaskTypePlanned.End = objPlannedEnd;
                        t.Planned = objTaskTypePlanned;
                    }

                    var objTempResult = JsonConvert.SerializeObject(t, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    TaskType objResult = JsonConvert.DeserializeObject<TaskType>(objTempResult);
                    //var objResult = JsonConvert.DeserializeObject(objTempResult);
                    //t.Planned = new TaskTypePlanned();
                    //t.Planned.Start = new TaskTypePlannedStart();

                    strFinLogMemo = DateTime.Now.ToString() + " TaskFetchRequest processed" + Environment.NewLine + strFinLogMemo;
                    Helper.UpdateFinLog(objContext, intFinIdLog, strFinLogMemo.Substring(0, Math.Min(strFinLogMemo.Length, 4000)));
                    string strJson = JsonConvert.SerializeObject(objTempResult);
                    Helper.UpdateInterfaceLog(objContext, "AO-Circet", "TF_RESPONSE", inId, orgId, strJson.Substring(0, Math.Min(strJson.Length, 4000)));
                    //ValidateModelStateAttribute validate = new ValidateModelStateAttribute();
                    //if (!TryValidateModel(t))
                    //{
                    //  Helper.UpdateInterfaceLog(objContext, "AO-Circet", "TF_RESPONSE_VALIDATION_ERROR", inId, orgId, ModelState.GetFieldValidationState("").ToString().Substring(0, Math.Min(ModelState.GetFieldValidationState("").ToString().Length, 4000)));
                    //}
                    return StatusCode(200, t);
                }
                else
                {
                    Helper.UpdateInterfaceLog(objContext, "AO-Circet", "TF_REQUEST_NOT_FOUND", inId, orgId, "StatusCode(204)");
                    return StatusCode(204);
                }
            }
            catch (Exception Ex)
            {
                if (Ex.Message.Substring(0, Math.Min(Ex.Message.Length, 5)) == "40025") { strError += " FIN_NAME NOT UNIQUE "; }
                if (intFinIdLog != 0) { strFinLogMemo = DateTime.Now.ToString() + Ex.Message + strError + Environment.NewLine + strFinLogMemo; Helper.UpdateFinLog(objContext, intFinIdLog, strFinLogMemo.Substring(0, Math.Min(strFinLogMemo.Length, 4000))); }
                Helper.UpdateInterfaceLog(objContext, "AO-Circet", "TF_REQUEST_ERROR", inId, orgId, "StatusCode 500: " + Ex.Message + strError);

                return StatusCode(500, Ex.Message + strError);
                //TODO: Uncomment the next line to return response 200 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
                // return StatusCode(200, default(TaskType));

                //TODO: Uncomment the next line to return response 400 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
                // return StatusCode(400, default(string));

                //TODO: Uncomment the next line to return response 401 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
                // return StatusCode(401);

                //TODO: Uncomment the next line to return response 403 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
                // return StatusCode(403);

                //TODO: Uncomment the next line to return response 404 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
                // return StatusCode(404);

                //TODO: Uncomment the next line to return response 429 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
                // return StatusCode(429);

                //TODO: Uncomment the next line to return response 500 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
                // return StatusCode(500);

                //TODO: Uncomment the next line to return response 503 or use other options such as return this.NotFound(), return this.BadRequest(..), ...
                // return StatusCode(503);
            }
            #endregion

        }
    }
}

