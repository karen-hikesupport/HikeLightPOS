using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace HikePOS.Services
{
	public class GetLocationService
	{
		static async Task<string> CallService(string strURL)
		{

			using (var httpClient = new HttpClient())
			{
				string strResult;
				try
				{
					strResult = await httpClient.GetStringAsync(new Uri(strURL));
				}
				catch
				{
					strResult = "Exception";
				}
				return strResult;
			}

		}

		internal static async Task<LocationPredictionClass> LocationAutoComplete(string strFullURL)
		{
			LocationPredictionClass objLocationPredictClass = null;
			string strResult = await CallService(strFullURL);
			if (strResult != "Exception")
			{
				objLocationPredictClass = JsonConvert.DeserializeObject<LocationPredictionClass>(strResult);
			}

			//foreach (Prediction objPred in objLocationPredictClass.predictions)
			//{
			//	if (objPred.types[0] == "country")
			//	{
			//		objLocationPredictClass12.predictions.Add(objPred);
			//		objLocationPredictClass12.status = "OK";
			//	}
			//}


			return objLocationPredictClass;
		}

		internal static async Task<PlaceDetail> PlaceDetail(string strFullURL)
		{
			PlaceDetail objPlaceDetail = null;
			string strResult = await CallService(strFullURL);
			if (strResult != "Exception")
			{
				objPlaceDetail = JsonConvert.DeserializeObject<PlaceDetail>(strResult);
			}

			return objPlaceDetail;
		}

	}
}
