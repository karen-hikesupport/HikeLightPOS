using System;
using System.Collections.ObjectModel;

namespace HikePOS.Models
{
	public class ListResultDto<T>
	{
		public ListResultDto()
		{
			items = new ObservableCollection<T>();
		}

		public int totalCount { get; set; }
		public ObservableCollection<T> items { get; set; }
	}

	public class ListResponseModel<T> : AjaxResponse
	{
		public ListResponseModel()
		{
			result = new ListResultDto<T>();
		}
		public ListResultDto<T> result { get; set; }
	}

	public class ResponseListModel<T> : AjaxResponse
	{
		public ResponseListModel()
		{
			result = new ObservableCollection<T>();
		}
		public ObservableCollection<T> result { get; set; }
	}

	public class ResponseModel<T> : AjaxResponse
	{
		public T result { get; set; }
	}

	public class RequestModel<T>
	{
		public T input { get; set; }
	}
    public class ListResult<T>
    {
        public ListResult()
        {
            items = new ObservableCollection<T>();
        }

        public int Count { get; set; }
        public ObservableCollection<T> items { get; set; }
    }

}
