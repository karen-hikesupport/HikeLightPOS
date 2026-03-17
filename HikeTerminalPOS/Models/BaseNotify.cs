using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Realms;
using Realms.Schema;
using Realms.Weaving;

namespace HikePOS.Models
{
 //   public partial class RealMCl : IRealmObject
	//{

 //       bool IRealmObjectBase.IsManaged => throw new NotImplementedException();

 //       bool IRealmObjectBase.IsValid => throw new NotImplementedException();

 //       bool IRealmObjectBase.IsFrozen => throw new NotImplementedException();

 //       Realm IRealmObjectBase.Realm => throw new NotImplementedException();

 //       ObjectSchema IRealmObjectBase.ObjectSchema => throw new NotImplementedException();

 //       DynamicObjectApi IRealmObjectBase.DynamicApi => throw new NotImplementedException();

 //       int IRealmObjectBase.BacklinksCount => throw new NotImplementedException();


 //   }
    public partial class BaseNotify :  INotifyPropertyChanged 
    {
		public event PropertyChangedEventHandler PropertyChanged;

        internal bool SetPropertyChanged<T>(ref T currentValue, T newValue, [CallerMemberName] string propertyName = "")
		{
			return PropertyChanged.SetProperty(this, ref currentValue, newValue, propertyName);
		}

		internal void SetPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

	}
}

namespace System.ComponentModel
{
	public static class BaseNotify
	{
		//Just adding some new funk.tionality to System.ComponentModel
		public static bool SetProperty<T>(this PropertyChangedEventHandler handler, object sender, ref T currentValue, T newValue, [CallerMemberName] string propertyName = "")
		{
			if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
				return false;

			currentValue = newValue;

			if (handler == null)
				return true;

			handler.Invoke(sender, new PropertyChangedEventArgs(propertyName));
			return true;
		}
	}
}
