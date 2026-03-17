using System;
using System.Threading.Tasks;
using HikePOS.Models;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace HikePOS.Services
{
	public class LastSyncService
	{
		public bool CreateUpdateLastSyncData(LastSyncDto objLastSyncDto)
		{
			try
			{
                if (objLastSyncDto != null)
                {
                    using var realm1 = RealmService.GetRealm();
                    realm1.Write(() =>
                    {
                        realm1.Add(objLastSyncDto.ToModel(), update: true);
                    });
                    return true;
                }
			}
            catch(KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
            }
			catch (Exception ex)
			{
                ex.Track();
			}
            return false;
		}

		public LastSyncDto GetLastSyncTime()
		{
			try
			{
                using var realm = RealmService.GetRealm();
                var data = realm.All<LastSyncDB>().FirstOrDefault();
                return LastSyncDto.FromModel(data);
            }
			catch (Exception ex)
			{
				ex.Track();
				return null;
			}
		}
	}
}
