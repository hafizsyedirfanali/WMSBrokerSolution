﻿using WMSBrokerProject.Interfaces;
using WMSBrokerProject.Models;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using Newtonsoft.Json;
using System;
using WMSBrokerProject.ConfigModels;

namespace WMSBrokerProject.Repositories
{
    public class GoEfficientServiceImplementation : IGoEfficientService
    {
        private readonly string? templateFolder;
        private readonly string symbolForConcatenation;
        private readonly string symbolForPriority;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly GoEfficientCredentials goEfficientCredentials;
        public GoEfficientServiceImplementation(IConfiguration configuration, IWebHostEnvironment hostEnvironment,
            IOptions<GoEfficientCredentials> goEfficientCredentials)
        {
            _configuration = configuration;
            this.hostEnvironment = hostEnvironment;
            this.goEfficientCredentials = goEfficientCredentials.Value;
            this.symbolForConcatenation = configuration.GetSection("MijnAansluiting:SymbolForConcatenation").Value ?? "+";
            this.symbolForPriority = configuration.GetSection("MijnAansluiting:SymbolForPriority").Value ?? "*";
            this.templateFolder = configuration.GetSection("TemplatesFolder").Value;
        }
        public string? GetHighestPriorityKey(TaskFetchResponse2Model model, string sourceKey, string destinationKey)
        {
            string? priorityKey = null;
            var sourceKeyArray = sourceKey.Split(symbolForPriority);
            foreach (var sKey in sourceKeyArray.OrderByDescending(s => s))
            {
                var value = GetValueOfKey(model, sKey);
                if (value is not null)
                {
                    priorityKey = sKey;//check
                }
            }
            return priorityKey;
        }
        public object? GetValueOfKey(TaskFetchResponse2Model model, string sourceKey)
        {
            object? value = null;
            model.WMSBeheerderAttributes.TryGetValue(sourceKey, out value);
            return value;
        }
        
        public (string DestinationKey, object? Value) GetOneToOneValue(TaskFetchResponse2Model model, string sourceKey, string destinationKey)
        {
            object? value;
            model.WMSBeheerderAttributes.TryGetValue(sourceKey, out value);
            return (destinationKey, value);
        }
        
        public DateTime GetFridayFromDate(int weekNumber, int year)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            DateTime firstMonday = jan1.AddDays((int)DayOfWeek.Monday - (int)jan1.DayOfWeek + (jan1.DayOfWeek <= DayOfWeek.Monday ? 0 : 7));
            DateTime desiredMonday = firstMonday.AddDays((weekNumber - 1) * 7);
            return desiredMonday.AddDays(4);
        }

        public async Task<ResponseModel<string>> GetKeyForRES4Mapping()
        {
            var responseModel = new ResponseModel<string>();
            try
            {
                var goEfficientMijnAansluitingMap = _configuration.GetSection("MijnAansluitingRES4Mapping").AsEnumerable();
                foreach (var attribute in goEfficientMijnAansluitingMap)
                {
                    if (attribute.Value != null)
                    {
                        var key = attribute.Key;//Its key represents RHS
                        var keyArray = key.Split(':');//in this array last but one will be key
                        var sourceKey = attribute.Value;//value is source key
                        var destinationKey = keyArray[keyArray.Length - 1];
                        responseModel.Result = destinationKey;
                    }
                }
                responseModel.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10017;
            }
            return responseModel;

        }
        public async Task<ResponseModel<string>> GetKeyForValueInRES3aMapping(string value)
        {
            var responseModel = new ResponseModel<string>();
            try
            {
                var goEfficientMijnAansluitingMap = _configuration.GetSection("MijnAansluitingRES3aMapping").AsEnumerable();
                foreach (var attribute in goEfficientMijnAansluitingMap)
                {
                    if (attribute.Value != null)
                    {
                        var key = attribute.Key;//Its key represents RHS
                        var keyArray = key.Split(':');//in this array last but one will be key
                        var sourceKey = attribute.Value;//value is source key
                        var destinationKey = keyArray[keyArray.Length - 1];
                        if (value.Contains(sourceKey))
                        {
                            responseModel.Result = destinationKey;
                        }
                    }
                }
                responseModel.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10017;
            }
            return responseModel;
        }
        public async Task<ResponseModel<RES4aTemplate>> FillDataIn4aTemplate(RES4aTemplate template, TaskFetchResponse2Model model)
		{
			var responseModel = new ResponseModel<RES4aTemplate>();
            try
            {
                var goEfficientMijnAansluitingMap = _configuration.GetSection("WMSBeheerderRES2Mapping").AsEnumerable();
                
                Dictionary<string, object?> mappedValues = new();
                foreach (var attribute in goEfficientMijnAansluitingMap)
                {
                    if (attribute.Value != null)
                    {
                        var key = attribute.Key;//Its key represents RHS
                        var keyArray = key.Split(':');//in this array last but one will be key
                        var sourceKey = attribute.Value;//value is source key
                        var destinationKey = keyArray[keyArray.Length - 1];

						var valueTuple = GetOneToOneValue(model, sourceKey, destinationKey);
						
						mappedValues.Add(valueTuple.DestinationKey, valueTuple.Value);

					}
                }


                template.GoEfficientTemplateValues = mappedValues;
                responseModel.Result = template;
                responseModel.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10017;
            }
            return responseModel;
        }

        public async Task<ResponseModel<RES4aTemplate>> FillFCDataIn4aTemplate(RES4aModel res4aModel, TaskFetchResponse2Model model)
        {
            var responseModel = new ResponseModel<RES4aTemplate>();
            try
            {
                var goEfficientMijnAansluitingMap = _configuration.GetSection("WMSBeheerderRES2FCMapping").AsEnumerable();
                if(res4aModel.Template is null) res4aModel.Template = new RES4aTemplate();
                Dictionary<string, object?> mappedValues = new();
                var fcMapping = res4aModel.FinNameFCList;
                foreach (var attribute in goEfficientMijnAansluitingMap)
                {
                    if (attribute.Value != null)
                    {
                        var key = attribute.Key;//Its key represents RHS
                        var keyArray = key.Split(':');//in this array last but one will be key
                        var sourceKey = attribute.Value;//value is source key
                        var destinationKey = keyArray[keyArray.Length - 1];
                        if(fcMapping.Where(s => s.FinName == destinationKey).Any())
                        {
                            Dictionary<string, string>? finNameSelectList = fcMapping.Where(s => s.FinName == destinationKey).Select(s => s.SelectListItems).FirstOrDefault();
                            if(finNameSelectList is not null)
                            {
                                var valueTuple = GetOneToOneValue(model, sourceKey, destinationKey);
                                if(finNameSelectList.Any(s=>s.Key == valueTuple.Value.ToString()))
                                {
                                    string? fcValue = finNameSelectList.Where(s => s.Key == valueTuple.Value.ToString()).Select(s => s.Value).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(fcValue))
                                    {
                                        mappedValues.Add(valueTuple.DestinationKey, fcValue);
                                    }
                                    else
                                    {
                                        responseModel.ErrorMessage = $"The key: {valueTuple.Value} is not present in GoEfficient";
                                        responseModel.ErrorCode = 10021;
                                    }
                                }
                                else
                                {
                                    responseModel.ErrorMessage = $"The key: {valueTuple.Value} is not present in GoEfficient";
                                    responseModel.ErrorCode = 10020;
                                }
                            }
                            else
                            {
                                responseModel.ErrorMessage = $"Select List not present for FIN_NAME of FC in GoEfficient";
                                responseModel.ErrorCode = 10022;
                            }
                        }
                        else
                        {
                            responseModel.ErrorMessage = $"FIN_NAME of FC not present in GoEfficient";
                            responseModel.ErrorCode = 10023;
                        }
                    }
                }
                if (mappedValues.Any())
                {
                    
                }
                foreach (var map in mappedValues)
                {
                    res4aModel.Template.GoEfficientTemplateValues.Add(map.Key, map.Value);
                }
                responseModel.Result = res4aModel.Template;
                responseModel.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10017;
            }
            return responseModel;
        }
        public async Task<ResponseModel<RES4aTemplate>> FillDataIn4aAddressTemplate(RES4aTemplate template, TaskFetchResponse2Model model)
        {
            var responseModel = new ResponseModel<RES4aTemplate>();
            try
            {
                var goEfficientMijnAansluitingMap = _configuration.GetSection("WMSBeheerderRES2AddressMapping").AsEnumerable();
                
                Dictionary<string, object?> mappedValues = new();
                foreach (var attribute in goEfficientMijnAansluitingMap)
                {
                    if (attribute.Value != null)
                    {
                        var key = attribute.Key;//Its key represents RHS
                        var keyArray = key.Split(':');//in this array last but one will be key
                        var sourceKey = attribute.Value;//value is source key
                        var destinationKey = keyArray[keyArray.Length - 1];
                        var valueTuple = GetOneToOneValue(model, sourceKey, destinationKey);
                        mappedValues.Add(valueTuple.DestinationKey, valueTuple.Value);                        
                    }
				}
                template.GoEfficientAddressTemplateValues = mappedValues;
                responseModel.Result = template;
                responseModel.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10017;
            }
            return responseModel;
        }
        public async Task<ResponseModel<Dictionary<string, string>>> GetGoEfficientFileAttributes()
        {
            var responseModel = new ResponseModel<Dictionary<string, string>>();
            try
            {
                var goEfficientAttributes = _configuration.GetSection("GoEfficientFileAttributes").AsEnumerable();
                Dictionary<string, string> goEfficientDict = new();
                foreach (var attribute in goEfficientAttributes)
                {
                    if (attribute.Value != null)
                    {
                        var key = attribute.Key;
                        var keyArray = key.Split(':');//in this array last but one will be key
                        var value = attribute.Value;
                        var leftKey = keyArray[keyArray.Length - 1];
                        goEfficientDict.Add(leftKey, value);
                    }
                }
                responseModel.Result = goEfficientDict;
                responseModel.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10016;
            }
            return responseModel;
        }
        public async Task<ResponseModel<Dictionary<string, string>>> GetGoEfficientAttributes()
        {
            var responseModel = new ResponseModel<Dictionary<string, string>>();
            try
            {
                var goEfficientAttributes = _configuration.GetSection("GoEfficientAttributes").AsEnumerable();
                Dictionary<string, string> goEfficientDict = new();
                foreach (var attribute in goEfficientAttributes)
                {
                    if (attribute.Value != null)
                    {
                        var key = attribute.Key;
                        var keyArray = key.Split(':');//in this array last but one will be key
                        var value = attribute.Value;
                        var leftKey = keyArray[keyArray.Length - 1];
                        goEfficientDict.Add(leftKey, value);
                    }
                }
                responseModel.Result = goEfficientDict;
                responseModel.IsSuccess = true;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10016;
            }
            return responseModel;
        }

        public async Task<ResponseModel<RES4Model>> REQ4_GetProIDAsync(REQ4Model model)
        {
            var responseModel = new ResponseModel<RES4Model>();

            try
            {
                using HttpClient client = new HttpClient();
                var date = DateTime.Now;
                var year = date.ToString("yyyy");
                var year_week = GetYearWeekISO(DateTime.Now);
                var yearweek = year + "-" + year_week;
                
                string? requestUri = _configuration.GetSection("GoEfficient:EndPointUrl").Value;

                string xmlRequest4 = string.Empty;
                var houseNumberExtension = string.IsNullOrEmpty(model.HouseNumberExtension) ? "" : model.HouseNumberExtension + " ";
                //var houseNumberSuffix = string.IsNullOrEmpty(model.HouseNumberSuffix) ? "" : model.HouseNumberSuffix + " ";


                xmlRequest4 = $@"<Request>
                                     {GetXMLHeader(model.InId)}
                                     <Body>
                                         <CreateOperation>
                                             <OperationName>PRO_CREATE_TREE_FROM_TEMPL</OperationName>
                                             <Values>
                                                 <Value FieldName=""PRO.PRO_ID"">{model.PRO_ID}</Value>
                                                 <Value FieldName=""Indicator"">{year};{year_week};{model.CityName} {model.StreetName} {model.HouseNumber} {houseNumberExtension}{model.PostalCode} {model.InId}</Value>
                                                 <Value FieldName=""Indicator2"">{model.Indicator2}</Value>
                                                 <Value FieldName=""Indicator3"">P</Value>
                                             </Values>
                                         </CreateOperation>
                                     </Body>
                                 </Request>";

				var content = new StringContent(xmlRequest4, Encoding.UTF8, "application/xml");
                string xmlResponse;
                if (!string.IsNullOrEmpty(requestUri))
                {
                    var response = await client.PostAsync(requestUri, content);
                    response.EnsureSuccessStatusCode();
                    xmlResponse = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var xmlResponseFilePath = Path.Combine(templateFolder!, $"GoEfficient_InstanceTemplateResponse_RES04.xml");
                    xmlResponse = File.ReadAllText(xmlResponseFilePath);
                }
                
                XDocument xdoc = XDocument.Parse(xmlResponse);

                var proId3Value = xdoc.Descendants("Value")
                                      .FirstOrDefault(e => (string)e.Attribute("FieldName") == "PRO.PRO_ID_3")?.Value;

                if (proId3Value != null)
                {
                    responseModel.Result = new RES4Model { ProId3 = proId3Value };
                    responseModel.IsSuccess = true;
                }
                else
                {
                    responseModel.ErrorMessage = "PRO.PRO_ID_3 not found.";
                    responseModel.ErrorCode = 10003;
                }
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10004;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10005;
            }
            return responseModel;
        }

        //in REQ4aModel Pass the mapped properties of GoEfficient
        public async Task<ResponseModel<RES4aModel>> REQ4a_GetTemplateFromGoEfficient(REQ4aModel model)//This service is to get the form template
        {
            var responseModel = new ResponseModel<RES4aModel>();

            try
            {
                using HttpClient client = new HttpClient();

                string? requestUri = _configuration.GetSection("GoEfficient:EndPointUrl").Value;

                string xmlRequest4a = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
                                        <Request>
	                                        {GetXMLHeader(model.InId)}
	                                        <Body>
		                                        <ReadOperation>
			                                        <Fields>
				                                        <Field>FIN.FIN_ID</Field>
				                                        <Field>UDF.UDF_TYPE</Field>
				                                        <Field>FIN.FIN_NAME</Field>
				                                        <Field>FIN.FIN_RECORD_ID</Field>
				                                        <Field>FIN.FIN_PATH</Field>
				                                        <Field>FIN.FIN_DATE</Field>
				                                        <Field>FIN.FIN_NUMBER</Field>
				                                        <Field>FIN.FIN_MEMO</Field>
				                                        <Field>FIN.FIN_FILE_EXT</Field>
				                                        <Field>UDF.UDF_TYPEINFO</Field>
				                                        <Field>UDF.UDF_LABEL</Field>
				                                        <Field>PRO.PRO_ID</Field>
			                                        </Fields>
			                                        <Conditions>
				                                        <Condition RightVariableType=""LiteralValue"" RightValue=""{model.ProId}"" Operator=""Equal"" LeftVariableType=""Field"" LeftValue=""PRO.PRO_ID""/>
			                                        </Conditions>
			                                        <OperationName>FIN_PRO_READ</OperationName>
		                                        </ReadOperation>
	                                        </Body>
                                        </Request>";

                var content = new StringContent(xmlRequest4a, Encoding.UTF8, "application/xml");
                string xmlResponse;
                if (!string.IsNullOrEmpty(requestUri))
                {
                    var response = await client.PostAsync(requestUri, content);
                    response.EnsureSuccessStatusCode();
                    xmlResponse = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var xmlResponseFilePath = Path.Combine(templateFolder!, $"GoEfficient_InstantiatedAttacmentsResponse_RES04a.xml");
                    xmlResponse = File.ReadAllText(xmlResponseFilePath);
                }

                RES4aTemplate template = new RES4aTemplate();
                List<GoEfficientTemplateAttributesClass> templateAttributeList = new();
                XDocument xdoc = XDocument.Parse(xmlResponse);

                foreach (var property in model.GoEfficientAttributes)
                {
                    //if we want to get all attributes without address we can do it here
                    XElement? rowElement = xdoc.Descendants("Row")
                                .FirstOrDefault(row =>
                                    row.Elements("Value")
                                    .Any(e => (string)e.Attribute("FieldName")! == "FIN.FIN_NAME" &&
                                    e.Value.ToLower() == property.Value.ToLower()));
                    if (rowElement is not null)
                    {
                        string finId = rowElement.Elements("Value").FirstOrDefault(x => x.Attribute("FieldName")?.Value == "FIN.FIN_ID")?.Value!;
                        string finName = rowElement.Elements("Value").FirstOrDefault(x => x.Attribute("FieldName")?.Value == "FIN.FIN_NAME")?.Value!;
                        string proId = rowElement.Elements("Value").FirstOrDefault(x => x.Attribute("FieldName")?.Value == "PRO.PRO_ID")?.Value!;
                        string udfType = rowElement.Elements("Value").FirstOrDefault(x => x.Attribute("FieldName")?.Value == "UDF.UDF_TYPE")?.Value!;
                        string udfTypeInfo = rowElement.Elements("Value").FirstOrDefault(x => x.Attribute("FieldName")?.Value == "UDF.UDF_TYPEINFO")?.Value!;

                        templateAttributeList.Add(new GoEfficientTemplateAttributesClass
                        {
                            GoEfficientAttributeName = property.Key,
                            MappingName = property.Value,
                            FinId = finId,
                            FinName = finName,
                            ProId = proId,
                            UdfType = udfType,
                            UdfTypeInfo = udfTypeInfo
                        });
                    }
                }
                template.GoEfficientTemplateAttributeList = templateAttributeList;


                //2. fill the values from res2 into above


                List<RES4aAddress> addresses = (from row in xdoc.Descendants("Row")
                                                let udfTypeValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "UDF.UDF_TYPE")?.Value
                                                where udfTypeValue == "A"
                                                let udfLabelValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "UDF.UDF_LABEL")?.Value
                                                let finNameValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "FIN.FIN_NAME")?.Value
                                                let udfFIN_IDValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "FIN.FIN_ID")?.Value
                                                select new RES4aAddress
                                                {
                                                    HouseNo = udfLabelValue,
                                                    FIN_Id = udfFIN_IDValue,
                                                    FIN_Name = finNameValue
                                                }).ToList();

                //var fixedContentList = (from row in xdoc.Descendants("Row")
                //                                let udfTypeValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "UDF.UDF_TYPE")?.Value
                //                                where udfTypeValue == "FC"
                //                                let finNameValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "FIN.FIN_NAME")?.Value
                //                                let udfTypeInfoValue = row.Elements("Value").FirstOrDefault(e => e.Attribute("FieldName")?.Value == "UDF.UDF_TYPEINFO")?.Value
                //                                select new 
                //                                {
                //                                    FIN_NAME = finNameValue,
                //                                    UDF_TYPEINFO = udfTypeInfoValue
                //                                }).ToList();
                var fixedContentList = templateAttributeList.Where(s => s.UdfType == "FC").ToList();
                List<FinNameFC> finNameFCList = new();
                foreach (var fc in fixedContentList)
                {
                    Dictionary<string, string> selectOptions = new();
                    var decodedUDFTypeInfo = System.Net.WebUtility.HtmlDecode(fc.UdfTypeInfo);
                    //SEL:
                    //<A>=Aansluitingen.nl;
                    //<ISP>=ISP;
                    //<CAIW>=Caiway;
                    //<WEB>=Web CIF;
                    //<M>=Mail;
                    //<LIP>=LIP aanvraagnummer;
                    //<COMB>=Combi projectnummer;
                    if (decodedUDFTypeInfo.StartsWith("SEL:", StringComparison.OrdinalIgnoreCase))
                    {
                        decodedUDFTypeInfo = decodedUDFTypeInfo.Substring("SEL:".Length);
                    }
                    var keyValuePairs = decodedUDFTypeInfo.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Split('=')).ToList();
                    Dictionary<string, string> selectListItems = new();
                    foreach (var keyValuePair in keyValuePairs)
                    {
                        if (keyValuePair.Length == 2)
                        {
                            var value = keyValuePair[0].Trim();
                            value = value.Replace("<", "").Replace(">", "").Trim();
                            var text = keyValuePair[1].Trim();
                            selectListItems.Add(key: text, value: value);
                        }
                    }
                    finNameFCList.Add(new FinNameFC
                    {
                        FinName = fc.FinName,
                        SelectListItems = selectListItems
                    });
                }

                //1.create TemplateAttributes for all addresses "AddressTemplateAttribute"
                Dictionary<string, string> addressTemplateAttribute = new Dictionary<string, string>();
                foreach (var property in model.GoEfficientAttributes)
                {
                    var isAvailableInRes4a = addresses.Where(s => s.FIN_Name == property.Value).Any();
                    if (isAvailableInRes4a)
                    {
                        addressTemplateAttribute.Add(property.Key, property.Value);
                    }
                }
                template.GoEfficientAddressTemplateAttributes = addressTemplateAttribute;



                responseModel.Result = new RES4aModel
                {
                    Template = template,
                    //Addresses = addresses,
                    FIN_ID = addresses.Select(s => s.FIN_Id).FirstOrDefault(),
                    FinNameFCList = finNameFCList
                };
                responseModel.IsSuccess = true;
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10006;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10007;
            }
            return responseModel;
        }

        public async Task<ResponseModel<RES5Model>> REQ5_SaveRecordToGoEfficient(REQ5Model model)
        {
            var responseModel = new ResponseModel<RES5Model>();

            try
            {
                using HttpClient client = new HttpClient();

                string? requestUri = _configuration.GetSection("GoEfficient:EndPointUrl").Value;

                string valueText = string.Empty;
                foreach (var templateField in model.GoEfficientTemplateValues)
                {
                    valueText += @$"<Value FieldName=""{templateField.Key}"">{templateField.Value}</Value>";
                }
                valueText += @"<Value FieldName=""Aanvraag ontvangen via?"">Aansluitingen.nl</Value>";
                string xmlRequest5 = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                                        <Request>
	                                        {GetXMLHeader(model.InId)}
	                                        <Body>
		                                        <UpdateOperation>
			                                        <OperationName>PRO_FIN_UPDATE</OperationName>
			                                        <Values>
                                                       {valueText}
			                                        </Values>
			                                        <Conditions>
				                                        <Condition FieldName=""PRO.PRO_ID"">{model.PRO_ID_3}</Condition>
				                                        <Condition FieldName=""Indicator"">R</Condition>
			                                        </Conditions>
		                                        </UpdateOperation>
	                                        </Body>
                                        </Request>";

                var content = new StringContent(xmlRequest5, Encoding.UTF8, "application/xml");
                string xmlResponse;
                if (!string.IsNullOrEmpty(requestUri))
                {
                    var response = await client.PostAsync(requestUri, content);
                    response.EnsureSuccessStatusCode();
                    xmlResponse = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var xmlResponseFilePath = Path.Combine(templateFolder!, $"GoEfficient_UpdateInstantiatedAttachmentsResponse_RES05.xml");
                    xmlResponse = File.ReadAllText(xmlResponseFilePath);
                }


                responseModel.Result = new RES5Model { };
                responseModel.IsSuccess = true;
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10008;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10009;
            }
            return responseModel;
        }

        public async Task<ResponseModel<RES5aModel>> REQ5a_SaveAddressToGoEfficient(REQ5aModel model)
        {
            var responseModel = new ResponseModel<RES5aModel>();

            try
            {
                using HttpClient client = new HttpClient();
                string? requestUri = _configuration.GetSection("GoEfficient:EndPointUrl").Value;

                string valueText = string.Empty;
                foreach (var templateField in model.Template.GoEfficientAddressTemplateValues)
                {
                    valueText += @$"<Value FieldName=""{templateField.Key}"">{templateField.Value}</Value>";
                }
                var dict = model.Template.GoEfficientAddressTemplateValues;
                dict.TryGetValue("ADRESS.ADRESS_STREET", out object? straat);
                dict.TryGetValue("ADRESS.ADRESS_HOUSNR", out object? huisnummer);
                dict.TryGetValue("ADRESS.ADRESS_HOUSNR_SFX", out object? huisnummerToevoeging);
                dict.TryGetValue("ADRESS.ADRESS_ZIPCODE", out object? postcode);
                dict.TryGetValue("ADRESS.ADRESS_TOWN", out object? plaats);
                valueText += @"<Value FieldName=""ADRESS.ADRESS_CNTR_ISO3166A3"">NLD</Value>";
                valueText += @$"<Value FieldName=""FIN.FIN_PATH"">{straat} {huisnummer} {huisnummerToevoeging} {postcode} {plaats}</Value>";

                string xmlRequest5a = @$"<?xml version=""1.0"" encoding=""utf-8""?>
                            <Request>
	                            {GetXMLHeader(model.InId)}
	                            <Body>
		                            <UpdateOperation>
			                            <OperationName>FIN_UPDATE_V1</OperationName>
			                            <Values>
				                           {valueText}
                                            <Value FieldName=""ADRESS.ADRESS_STATE""/>
				                            <Value FieldName=""ADRESS.ADRESS_POSTAL_BOX""/>
			                            </Values>
			                            <Conditions>
				                            <Condition FieldName=""FIN.FIN_ID"">{model.Address_FIN_ID}</Condition>
			                            </Conditions>
		                            </UpdateOperation>
	                            </Body>
                            </Request>";

                var content = new StringContent(xmlRequest5a, Encoding.UTF8, "application/xml");

                string xmlResponse;
                if (!string.IsNullOrEmpty(requestUri))
                {
                    var response = await client.PostAsync(requestUri, content);
                    response.EnsureSuccessStatusCode();
                    xmlResponse = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var xmlResponseFilePath = Path.Combine(templateFolder!, $"GoEfficient_UpdateInstantiatedAddressAttachmentsResponse_RES05a.xml");
                    xmlResponse = File.ReadAllText(xmlResponseFilePath);
                }


                responseModel.Result = new RES5aModel { };
                responseModel.IsSuccess = true;
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10010;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10011;
            }
            return responseModel;
        }

        public async Task<ResponseModel<RES5bModel>> REQ5b_AddFilesToGoEfficient(REQ5bModel model)
        {
            var responseModel = new ResponseModel<RES5bModel>();

            try
            {
                using HttpClient client = new HttpClient();

                string? requestUri = _configuration.GetSection("GoEfficient:EndPointUrl").Value;

                string xmlRequest5b = $@"<Request>
                                      {GetXMLHeader(model.AanvraagId)}                               
                                      <Body>
                                       <UpdateOperation>
                                           <OperationName>FIN_UPDATE_V1</OperationName>
                                           <Values> 
                                               <Value FieldName=""FIN.FIN_PATH"">{model.Fin_Path}</Value>
                                               <Value FieldName=""FIN.FIN_CONTENTS"">{model.Fin_Content}</Value>
                                           </Values>
                                           <Conditions>
                                                <Condition FieldName=""FIN.FIN_ID"">{model.Fin_Id}</Condition>
                                           </Conditions>
                                       </UpdateOperation>
                                       </Body>
                                  </Request>";

                var content = new StringContent(xmlRequest5b, Encoding.UTF8, "application/xml");
                
                string xmlResponse;
                if (!string.IsNullOrEmpty(requestUri))
                {
                    var response = await client.PostAsync(requestUri, content);
                    response.EnsureSuccessStatusCode();
                    xmlResponse = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var xmlResponseFilePath = Path.Combine(templateFolder!, $"GoEfficient_Request05bUpdateInstantiatedFileContentsAttachments_RES05b.xml");
                    xmlResponse = File.ReadAllText(xmlResponseFilePath);
                }

                responseModel.Result = new RES5bModel { };
                responseModel.IsSuccess = true;
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10008;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10009;
            }
            return responseModel;
        }

        public async Task<ResponseModel<RES6Model>> REQ6_IsRecordExist(REQ6Model model)
        //It is used to check if the record already exist in GoEfficient
        {
            var responseModel = new ResponseModel<RES6Model>();

            try
            {
                using HttpClient client = new HttpClient();
                string? requestUri = _configuration.GetSection("GoEfficient:EndPointUrl").Value;

                string xmlRequest6 = @$"<Request>
                                        {GetXMLHeader(model.InId)}
                                        <Body>
                                            <ReadOperation>
                                                <Fields>
                                                    <Field>PRO.PRO_ID</Field>
                                                </Fields>
                                                <Conditions>
                                                    <Condition RightVariableType=""LiteralValue"" RightValue=""{model.InId}"" Operator=""Equal"" LeftVariableType=""Field"" LeftValue=""FIN.FIN_PATH""/>
                                                    <Condition RightVariableType=""LiteralValue"" RightValue=""{model.Huurder_UDF_Id}"" Operator=""Equal"" LeftVariableType=""Field"" LeftValue=""FIN.FIN_UDF_ID""/>
                                                    <Condition RightVariableType=""LiteralValue"" RightValue=""'cifwms-huurderid'"" Operator=""Equal"" LeftVariableType=""Field"" LeftValue=""FIN.FIN_NAME_L""/>
                                                </Conditions>
                                                <OperationName>PRO_READ_M_V1</OperationName>
                                            </ReadOperation>
                                        </Body>
                                    </Request>";

                var content = new StringContent(xmlRequest6, Encoding.UTF8, "application/xml");

                string xmlResponse;
                if (!string.IsNullOrEmpty(requestUri))
                {
                    var response = await client.PostAsync(requestUri, content);
                    response.EnsureSuccessStatusCode();
                    xmlResponse = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var xmlResponseFilePath = Path.Combine(templateFolder!, $"GoEfficient_InstanceTemplateRequest_RES06.xml");
                    xmlResponse = File.ReadAllText(xmlResponseFilePath);
                }
                
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlResponse);
                string countValue = xmlDoc.SelectSingleNode("//Rows").Attributes["Count"].Value;
                int.TryParse(countValue, out int count);

                responseModel.Result = new RES6Model { IsRecordExist = count > 0 };
                responseModel.IsSuccess = true;
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10014;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 10015;
            }
            return responseModel;
        }

        public async Task<ResponseModel<CTRES7aModel>> REQ7a(CTREQ7aModel model)
        {
            var responseModel = new ResponseModel<CTRES7aModel>();
            try
            {
                using HttpClient client = new HttpClient();
                string? requestUri = _configuration.GetSection("GoEfficient:EndPointUrl").Value;
                var date = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

                var xmlRequest7a = @$"<?xml version=""1.0"" encoding=""UTF-8""?>
                                        <Request>
	                                        {GetXMLHeader(model.RequestId)}
                                            <Body>
                                                <UpdateOperation>
                                                    <OperationName>PRO_UPDATE_V1</OperationName>
                                                    <Values>
                                                        <Value FieldName=""PRO.PRO_OPENED"">{date}</Value>
                                                        <Value FieldName=""PRO.PRO_CLOSED"">{date}</Value>
                                                    </Values>
                                                    <Conditions>
                                                        <Condition FieldName=""PRO.PRO_ID"">{model.ProId}</Condition>
                                                    </Conditions>
                                                </UpdateOperation>
                                            </Body>
                                        </Request>";

                var content = new StringContent(xmlRequest7a, Encoding.UTF8, "application/xml");
                string xmlResponse;
                if (!string.IsNullOrEmpty(requestUri))
                {
                    var response = await client.PostAsync(requestUri, content);
                    response.EnsureSuccessStatusCode();
                    xmlResponse = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var xmlResponseFilePath = Path.Combine(templateFolder!, $"GoEfficient_RES7aOpenTask.xml");
                    xmlResponse = File.ReadAllText(xmlResponseFilePath);
                }


                XDocument doc = XDocument.Parse(xmlResponse);

                responseModel.Result = new CTRES7aModel { };
                responseModel.IsSuccess = true;
            }
            catch (HttpRequestException ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 40006;
            }
            catch (Exception ex)
            {
                responseModel.ErrorMessage = ex.Message;
                responseModel.ErrorCode = 40007;
            }
            return responseModel;
        }

        private string GetXMLHeader(string ind)
        {
            return $@"<Header>
                        <User>{goEfficientCredentials.Username}</User>
                        <PassWord>{goEfficientCredentials.Password}</PassWord>
                        <Culture>{goEfficientCredentials.Culture}</Culture>
		                <DataserverName>{goEfficientCredentials.DataserverName}</DataserverName>
                        <RequestId>{ind}</RequestId>
                        <ContinueOnError>true</ContinueOnError>
                    </Header>";
        }
        private string GetYearWeekISO(DateTime date)
        {
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar calendar = dfi.Calendar;

            int week = calendar.GetWeekOfYear(date, dfi.CalendarWeekRule, DayOfWeek.Monday);

            // Handle the edge case where the day is in the first week of the year, but the year is the previous one.
            if (date.Month == 12 && week == 1)
            {
                return $"{date.Year + 1}-{week:00}";
            }
            return $"{date.Year}-{week:00}";
        }

		public async Task<ResponseModel<Dictionary<string, object>>> FillSourcePathInBeheerderAttributesDictionary(TaskFetchResponseModel model)
		{
			var responseModel = new ResponseModel<Dictionary<string, object>>();
			try
			{
				Dictionary<string, object?> beheerderAttributes = new Dictionary<string, object>();
				if (_configuration.GetSection("WMSBeheerderAttributes").GetChildren().Any(x => x.Key == model.action))
				{
					beheerderAttributes = _configuration.GetSection($"WMSBeheerderAttributes:{model.action}")
						.GetChildren()
						.ToDictionary(x => x.Key, x => (object?) x.Value);
				}

				responseModel.Result = beheerderAttributes;
				responseModel.IsSuccess = true;
			}			
			catch (Exception ex)
			{
				responseModel.ErrorMessage = ex.Message;
				responseModel.ErrorCode = 10018;
			}
			return responseModel;
		}
		public async Task<ResponseModel<Dictionary<string, object>>> FillDataInBeheerderAttributesDictionary(TaskFetchResponseModel model, Dictionary<string, object> sourcePathInBeheerderAttributesDictionary)
		{
			var responseModel = new ResponseModel<Dictionary<string, object>>();
			try
			{
				Dictionary<string, object?> beheerderData = new Dictionary<string, object>();
                foreach (var pair in sourcePathInBeheerderAttributesDictionary)
                {
                    var key = pair.Key;
                    var valuePath = pair.Value;
					var value = GetPropertyValueOrField(obj: model, propertyPath: valuePath.ToString());
                    beheerderData.Add(key, value);
				}

				responseModel.Result = beheerderData;
				responseModel.IsSuccess = true;
			}
			catch (Exception ex)
			{
				responseModel.ErrorMessage = ex.Message;
				responseModel.ErrorCode = 10019;
			}
			return responseModel;
		}

		public async Task<ResponseModel<REQ4Model>> FillDataForRequest4(Dictionary<string, object> dataDictionary)
		{
			var responseModel = new ResponseModel<REQ4Model>();
			try
			{
				var goEfficientWMSMap = _configuration.GetSection("WMSBeheerderRES4Mapping").AsEnumerable();

				Dictionary<string, object?> mappedValues = new();
				foreach (var attribute in goEfficientWMSMap)
				{
					if (attribute.Value != null)
					{
						var key = attribute.Key;//Its key represents RHS
						var keyArray = key.Split(':');//in this array last but one will be key
						
                        var sourceKey = attribute.Value;//value is source key
						var destinationKey = keyArray[keyArray.Length - 1];

						//var valueTuple = GetOneToOneValue(model, sourceKey, destinationKey);
						var isValueAvailable = dataDictionary.TryGetValue(sourceKey, out object? value);
						mappedValues.Add(destinationKey, value);
					}
				}
                mappedValues.TryGetValue("streetName", out object? streetName);
				mappedValues.TryGetValue("cityName", out object? cityName);
				mappedValues.TryGetValue("country", out object? country);
				mappedValues.TryGetValue("houseNumber", out object? houseNumber);
				mappedValues.TryGetValue("postalCode", out object? postalCode);
				mappedValues.TryGetValue("houseNumberExtension", out object? houseNumberExtension);
				//mappedValues.TryGetValue("streetName", out object? streetName);

				responseModel.Result = new REQ4Model
                {
                    StreetName = streetName!=null? streetName.ToString()!:"",
					CityName = cityName != null? cityName.ToString()!:"",
					Country = country != null? country.ToString()!:"",
					HouseNumber = houseNumber != null? houseNumber.ToString()!:"",
					PostalCode = postalCode != null? postalCode.ToString()!:"",
					HouseNumberExtension = houseNumberExtension != null? houseNumberExtension.ToString()!:""
				};
				responseModel.IsSuccess = true;
			}
			catch (Exception ex)
			{
				responseModel.ErrorMessage = ex.Message;
				responseModel.ErrorCode = 10017;
			}
			return responseModel;
		}

		#region Helper Functions of Response 2
		public object GetPropertyValueOrField(object obj, string propertyPath)
		{
			if (obj == null) throw new ArgumentNullException(nameof(obj));
			if (propertyPath == null) throw new ArgumentNullException(nameof(propertyPath));

			Type objType = obj.GetType();
			foreach (var part in propertyPath.Split('.'))
			{
				if (obj == null) { return null; }

				// Check if the part has an indexer
				var match = Regex.Match(part, @"(.*?)\[(\d+)\]");
				if (match.Success)
				{
					var propertyName = match.Groups[1].Value;
					var index = int.Parse(match.Groups[2].Value);

					obj = GetValue(obj, objType, propertyName);

					if (obj is IList list && index < list.Count)
					{
						if (list.Count > 1)
						{
							return null;
						}
						obj = list[index];
						var count = list.Count;
					}
					else
					{
						return null;
					}

					objType = obj?.GetType();
					continue;
				}

				obj = GetValue(obj, objType, part);

				objType = obj?.GetType();
			}
			return obj;
		}
		public int? GetArrayPropertyCount(object obj, string propertyPath)
		{
			if (obj == null) throw new ArgumentNullException(nameof(obj));
			if (propertyPath == null) throw new ArgumentNullException(nameof(propertyPath));

			Type objType = obj.GetType();
			foreach (var part in propertyPath.Split('.'))
			{
				if (obj == null) { return null; }

				var match = Regex.Match(part, @"(.*?)\[(.*?)\]");
				if (match.Success)
				{
					var propertyName = match.Groups[1].Value;

					obj = GetValue(obj, objType, propertyName);

					if (obj is IList list)
					{
						return list.Count;
					}
					else
					{
						return null;
					}
				}
				obj = GetValue(obj, objType, part);
				objType = obj?.GetType();
			}
			return null;
		}
		private object GetValue(object obj, Type objType, string part)
		{
			var propertyInfo = objType.GetProperty(part);
			if (propertyInfo != null)
			{
				return propertyInfo.GetValue(obj, null);
			}

			var fieldInfo = objType.GetField(part);
			if (fieldInfo != null)
			{
				return fieldInfo.GetValue(obj);
			}

			return null;
		}

		
		#endregion
	}


}
