//using Akavache;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System;
using System.Reactive;
using System.Diagnostics;

namespace HikePOS.Services
{
     public static class CommonQueries
    {
        /*
		 * Get items from the db
		 */


//        // Get all objects of type T
//        public static async Task<ObservableCollection<T>> GetAllLocals<T>()
//        {
//            try
//            {
//                var objects = await BlobCache.LocalMachine.GetAllObjects<T>();
//                return new ObservableCollection<T>(objects);
//            }
//            catch (KeyNotFoundException ex)
//            {
//                Debug.WriteLine("KeyNotFoundException in GetAllLocals : " + ex.Message + " : " + ex.StackTrace);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                Debug.WriteLine("Exception in GetAllLocals : " + ex.Message + " : " + ex.StackTrace);
//                return null;
//            }
//        }

//        // Get an object serialized via InsertObject
//        public async static Task<T> GetObject<T>(string key)
//        {

//            try
//            {
//                return await BlobCache.LocalMachine.GetObject<T>(key);
//            }
//            catch (KeyNotFoundException ex)
//            {
//                //ex.Track();
//                Debug.WriteLine(ex.Message);
//                return default(T);
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return default(T);
//            }
//        }

//        // Get an object serialized via InsertObject
//        public async static Task<T> GetObject<T>(int id)
//        {
//            //return await BlobCache.LocalMachine.GetObject<T>(nameof(T) + "_" + id.ToString());
//            try
//            {
//                return await BlobCache.LocalMachine.GetObject<T>(nameof(T) + "_" + id.ToString());
//            }
//            catch (KeyNotFoundException ex)
//            {
//                Debug.WriteLine(ex.Message);
//                return default(T);
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return default(T);
//            }
//        }


//        // Return a list of all keys. Use for debugging purposes only.
//        public async static Task<IEnumerable<string>> GetAllKeys()
//        {
//            try
//            {
//                return await BlobCache.LocalMachine.GetAllKeys();
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return null;
//            }
//        }

//        // Get a single item
//        static IObservable<byte[]> Get(string key)
//        {

//            try
//            {
//                return BlobCache.LocalMachine.Get(key);
//            }
//            catch (KeyNotFoundException ex)
//            {
//                //ex.Track();
//                Debug.WriteLine(ex.Message);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return null;
//            }
//        }

//        // Get a list of items
//        static IObservable<IDictionary<string, byte[]>> Get(IEnumerable<string> keys)
//        {
//            try
//            {
//                return BlobCache.LocalMachine.Get(keys);
//            }
//            catch (KeyNotFoundException ex)
//            {
//                //ex.Track();
//                Debug.WriteLine(ex.Message);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return null;
//            }
//        }





//        // Get a list of objects given a list of keys
//        static IObservable<IDictionary<string, T>> GetObjects<T>(IEnumerable<string> keys)
//        {
//            try
//            {
//                return BlobCache.LocalMachine.GetObjects<T>(keys);
//            }
//            catch (KeyNotFoundException ex)
//            {
//                //ex.Track();
//                Debug.WriteLine(ex.Message);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return null;
//            }
//        }


//        /*
//		 * Save items to the store
//		 */

//        // Insert a single item
//        static IObservable<Unit> Insert(string key, byte[] data, DateTimeOffset? absoluteExpiration = null)
//        {
//            try
//            {
//                return BlobCache.LocalMachine.Insert(key, data, absoluteExpiration);
//            }
//            catch (KeyNotFoundException ex)
//            {
//                //ex.Track();
//                Debug.WriteLine(ex.Message);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return null;
//            }
//        }

//        // Insert a set of items
//        static IObservable<Unit> Insert(IDictionary<string, byte[]> keyValuePairs, DateTimeOffset? absoluteExpiration = null)
//        {
//            try
//            {
//                return BlobCache.LocalMachine.Insert(keyValuePairs, absoluteExpiration);
//            }
//            catch (KeyNotFoundException ex)
//            {
//                //ex.Track();
//                Debug.WriteLine(ex.Message);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return null;
//            }
//        }

//        // Insert a single object
//        static IObservable<Unit> InsertObject<T>(string key, T value, DateTimeOffset? absoluteExpiration = null)
//        {
//            try
//            {
//                return BlobCache.LocalMachine.InsertObject<T>(key, value, absoluteExpiration);
//            }
//            catch (KeyNotFoundException ex)
//            {
//                //ex.Track();
//                Debug.WriteLine(ex.Message);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return null;
//            }
//        }


//        // Insert a group of objects
//        static IObservable<Unit> InsertObjects<T>(IDictionary<string, T> keyValuePairs, DateTimeOffset? absoluteExpiration = null)
//        {
//            try
//            {
//                return BlobCache.LocalMachine.InsertObjects<T>(keyValuePairs, absoluteExpiration);
//            }
//            catch (KeyNotFoundException ex)
//            {
//                //ex.Track();
//                Debug.WriteLine(ex.Message);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return null;
//            }
//        }

//        /*
//		 * Remove items from the store
//		 */

//        // Delete a single item
//        public static IObservable<Unit> Invalidate(string key)
//        {
//            try
//            {
//                return BlobCache.LocalMachine.Invalidate(key);
//            }
//            catch (KeyNotFoundException ex)
//            {
//                //ex.Track();
//                Debug.WriteLine(ex.Message);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return null;
//            }
//        }

//        // Delete a list of items
//        static IObservable<Unit> Invalidate(IEnumerable<string> keys)
//        {
//            try
//            {
//                return BlobCache.LocalMachine.Invalidate(keys);
//            }
//            catch (KeyNotFoundException ex)
//            {
//                //ex.Track();
//                Debug.WriteLine(ex.Message);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return null;
//            }
//        }

//        // Delete a single object (do *not* use Invalidate for items inserted with InsertObject!)
//        static IObservable<Unit> InvalidateObject<T>(string key)
//        {
//            try
//            {
//                return BlobCache.LocalMachine.InvalidateObject<T>(key);
//            }
//            catch (KeyNotFoundException ex)
//            {
//                //ex.Track();
//                Debug.WriteLine(ex.Message);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return null;
//            }
//        }

//        // Deletes a list of objects
//        static IObservable<Unit> InvalidateObjects<T>(IEnumerable<string> keys)
//        {
//            try
//            {
//                return BlobCache.LocalMachine.InvalidateObjects<T>(keys);
//            }
//            catch (KeyNotFoundException ex)
//            {
//                //ex.Track();
//                Debug.WriteLine(ex.Message);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return null;
//            }
//        }

//        // Deletes all items (regardless if they are objects or not)
//        public static IObservable<Unit> InvalidateAll()
//        {
//            try
//            {
//                return BlobCache.LocalMachine.InvalidateAll();
//            }
//            catch (KeyNotFoundException ex)
//            {
//                //ex.Track();
//                Debug.WriteLine(ex.Message);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return null;
//            }
//        }

//        // Deletes all objects of type T
//        public static IObservable<Unit> InvalidateAllObjects<T>()
//        {
//            try
//            {
//                return BlobCache.LocalMachine.InvalidateAllObjects<T>();
//            }
//            catch (KeyNotFoundException ex)
//            {
//                //ex.Track();
//                Debug.WriteLine(ex.Message);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return null;
//            }
//        }

//        /*
//		 * Get Metadata about items
//		 */



//        // Return the time which an item was created
//        static IObservable<DateTimeOffset?> GetCreatedAt(string key)
//        {
//            try
//            {
//                return BlobCache.LocalMachine.GetCreatedAt(key);
//            }
//            catch (KeyNotFoundException ex)
//            {
//                //ex.Track();
//                Debug.WriteLine(ex.Message);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return null;
//            }
//        }

//        // Return the time which an object of type T was created
//        static IObservable<DateTimeOffset?> GetObjectCreatedAt<T>(string key)
//        {
//            try
//            {
//                return BlobCache.LocalMachine.GetObjectCreatedAt<T>(key);
//            }
//            catch (KeyNotFoundException ex)
//            {
//                //ex.Track();
//                Debug.WriteLine(ex.Message);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return null;
//            }
//        }

//        // Return the time which a list of keys were created
//        static IObservable<IDictionary<string, DateTimeOffset?>> GetCreatedAt(IEnumerable<string> keys)
//        {
//            try
//            {
//                return BlobCache.LocalMachine.GetCreatedAt(keys);
//            }
//            catch (KeyNotFoundException ex)
//            {
//                //ex.Track();
//                Debug.WriteLine(ex.Message);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return null;
//            }
//        }

//        /*
//		 * Utility methods
//		 */

//        // Attempt to ensure all outstanding operations are written to disk
//        public static IObservable<Unit> Flush()
//        {
//            try
//            {
//                return BlobCache.LocalMachine.Flush();
//            }
//            catch (KeyNotFoundException ex)
//            {
//                //ex.Track();
//                Debug.WriteLine(ex.Message);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return null;
//            }
//        }

//        // Preemptively drop all expired keys and run SQLite's VACUUM method on the
//        // underlying database
//        static IObservable<Unit> Vacuum()
//        {
//            try
//            {
//                return BlobCache.LocalMachine.Vacuum();
//            }
//            catch (KeyNotFoundException ex)
//            {
//                //ex.Track();
//                Debug.WriteLine(ex.Message);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                ex.Track();
//                return null;
//            }
//        }
    }
}
