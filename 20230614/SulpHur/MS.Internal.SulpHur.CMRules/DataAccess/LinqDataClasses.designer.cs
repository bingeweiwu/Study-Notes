﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MS.Internal.SulpHur.CMRules.DataAccess
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="Sulphur")]
	public partial class LinqDataClassesDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertSpellIgnore(SpellIgnore instance);
    partial void UpdateSpellIgnore(SpellIgnore instance);
    partial void DeleteSpellIgnore(SpellIgnore instance);
    #endregion
		
		public LinqDataClassesDataContext() : 
				base(global::MS.Internal.SulpHur.CMRules.Properties.Settings.Default.SulphurConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public LinqDataClassesDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public LinqDataClassesDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public LinqDataClassesDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public LinqDataClassesDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<SpellFilter> SpellFilters
		{
			get
			{
				return this.GetTable<SpellFilter>();
			}
		}
		
		public System.Data.Linq.Table<SpellIgnore> SpellIgnores
		{
			get
			{
				return this.GetTable<SpellIgnore>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.SpellFilters")]
	public partial class SpellFilter
	{
		
		private int _SpellFilterID;
		
		private string _SpellFilterWord;
		
		private string _SpellFilterWordInString;
		
		public SpellFilter()
		{
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SpellFilterID", DbType="Int NOT NULL")]
		public int SpellFilterID
		{
			get
			{
				return this._SpellFilterID;
			}
			set
			{
				if ((this._SpellFilterID != value))
				{
					this._SpellFilterID = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SpellFilterWord", DbType="NChar(10) NOT NULL", CanBeNull=false)]
		public string SpellFilterWord
		{
			get
			{
				return this._SpellFilterWord;
			}
			set
			{
				if ((this._SpellFilterWord != value))
				{
					this._SpellFilterWord = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SpellFilterWordInString", DbType="NVarChar(MAX)")]
		public string SpellFilterWordInString
		{
			get
			{
				return this._SpellFilterWordInString;
			}
			set
			{
				if ((this._SpellFilterWordInString != value))
				{
					this._SpellFilterWordInString = value;
				}
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.SpellIgnores")]
	public partial class SpellIgnore : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _SpellIgnoreID;
		
		private string _ENUStr;
		
		private string _CHSStr;
		
		private string _DEUStr;
		
		private string _FRAStr;
		
		private string _JPNStr;
		
		private string _RUSStr;
		
		private string _CHTStr;
		
		private string _CSYStr;
		
		private string _ESNStr;
		
		private string _HUNStr;
		
		private string _ITAStr;
		
		private string _KORStr;
		
		private string _NLDStr;
		
		private string _PLKStr;
		
		private string _PTBStr;
		
		private string _PTGStr;
		
		private string _SVEStr;
		
		private string _TRKStr;
		
		private string _DANStr;
		
		private string _ELLStr;
		
		private string _FINStr;
		
		private string _NORStr;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnSpellIgnoreIDChanging(int value);
    partial void OnSpellIgnoreIDChanged();
    partial void OnENUStrChanging(string value);
    partial void OnENUStrChanged();
    partial void OnCHSStrChanging(string value);
    partial void OnCHSStrChanged();
    partial void OnDEUStrChanging(string value);
    partial void OnDEUStrChanged();
    partial void OnFRAStrChanging(string value);
    partial void OnFRAStrChanged();
    partial void OnJPNStrChanging(string value);
    partial void OnJPNStrChanged();
    partial void OnRUSStrChanging(string value);
    partial void OnRUSStrChanged();
    partial void OnCHTStrChanging(string value);
    partial void OnCHTStrChanged();
    partial void OnCSYStrChanging(string value);
    partial void OnCSYStrChanged();
    partial void OnESNStrChanging(string value);
    partial void OnESNStrChanged();
    partial void OnHUNStrChanging(string value);
    partial void OnHUNStrChanged();
    partial void OnITAStrChanging(string value);
    partial void OnITAStrChanged();
    partial void OnKORStrChanging(string value);
    partial void OnKORStrChanged();
    partial void OnNLDStrChanging(string value);
    partial void OnNLDStrChanged();
    partial void OnPLKStrChanging(string value);
    partial void OnPLKStrChanged();
    partial void OnPTBStrChanging(string value);
    partial void OnPTBStrChanged();
    partial void OnPTGStrChanging(string value);
    partial void OnPTGStrChanged();
    partial void OnSVEStrChanging(string value);
    partial void OnSVEStrChanged();
    partial void OnTRKStrChanging(string value);
    partial void OnTRKStrChanged();
    partial void OnDANStrChanging(string value);
    partial void OnDANStrChanged();
    partial void OnELLStrChanging(string value);
    partial void OnELLStrChanged();
    partial void OnFINStrChanging(string value);
    partial void OnFINStrChanged();
    partial void OnNORStrChanging(string value);
    partial void OnNORStrChanged();
    #endregion
		
		public SpellIgnore()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SpellIgnoreID", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int SpellIgnoreID
		{
			get
			{
				return this._SpellIgnoreID;
			}
			set
			{
				if ((this._SpellIgnoreID != value))
				{
					this.OnSpellIgnoreIDChanging(value);
					this.SendPropertyChanging();
					this._SpellIgnoreID = value;
					this.SendPropertyChanged("SpellIgnoreID");
					this.OnSpellIgnoreIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ENUStr", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string ENUStr
		{
			get
			{
				return this._ENUStr;
			}
			set
			{
				if ((this._ENUStr != value))
				{
					this.OnENUStrChanging(value);
					this.SendPropertyChanging();
					this._ENUStr = value;
					this.SendPropertyChanged("ENUStr");
					this.OnENUStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_CHSStr", DbType="NVarChar(50)")]
		public string CHSStr
		{
			get
			{
				return this._CHSStr;
			}
			set
			{
				if ((this._CHSStr != value))
				{
					this.OnCHSStrChanging(value);
					this.SendPropertyChanging();
					this._CHSStr = value;
					this.SendPropertyChanged("CHSStr");
					this.OnCHSStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DEUStr", DbType="NVarChar(50)")]
		public string DEUStr
		{
			get
			{
				return this._DEUStr;
			}
			set
			{
				if ((this._DEUStr != value))
				{
					this.OnDEUStrChanging(value);
					this.SendPropertyChanging();
					this._DEUStr = value;
					this.SendPropertyChanged("DEUStr");
					this.OnDEUStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_FRAStr", DbType="NVarChar(50)")]
		public string FRAStr
		{
			get
			{
				return this._FRAStr;
			}
			set
			{
				if ((this._FRAStr != value))
				{
					this.OnFRAStrChanging(value);
					this.SendPropertyChanging();
					this._FRAStr = value;
					this.SendPropertyChanged("FRAStr");
					this.OnFRAStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_JPNStr", DbType="NVarChar(50)")]
		public string JPNStr
		{
			get
			{
				return this._JPNStr;
			}
			set
			{
				if ((this._JPNStr != value))
				{
					this.OnJPNStrChanging(value);
					this.SendPropertyChanging();
					this._JPNStr = value;
					this.SendPropertyChanged("JPNStr");
					this.OnJPNStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_RUSStr", DbType="NVarChar(50)")]
		public string RUSStr
		{
			get
			{
				return this._RUSStr;
			}
			set
			{
				if ((this._RUSStr != value))
				{
					this.OnRUSStrChanging(value);
					this.SendPropertyChanging();
					this._RUSStr = value;
					this.SendPropertyChanged("RUSStr");
					this.OnRUSStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_CHTStr", DbType="NVarChar(50)")]
		public string CHTStr
		{
			get
			{
				return this._CHTStr;
			}
			set
			{
				if ((this._CHTStr != value))
				{
					this.OnCHTStrChanging(value);
					this.SendPropertyChanging();
					this._CHTStr = value;
					this.SendPropertyChanged("CHTStr");
					this.OnCHTStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_CSYStr", DbType="NVarChar(50)")]
		public string CSYStr
		{
			get
			{
				return this._CSYStr;
			}
			set
			{
				if ((this._CSYStr != value))
				{
					this.OnCSYStrChanging(value);
					this.SendPropertyChanging();
					this._CSYStr = value;
					this.SendPropertyChanged("CSYStr");
					this.OnCSYStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ESNStr", DbType="NVarChar(50)")]
		public string ESNStr
		{
			get
			{
				return this._ESNStr;
			}
			set
			{
				if ((this._ESNStr != value))
				{
					this.OnESNStrChanging(value);
					this.SendPropertyChanging();
					this._ESNStr = value;
					this.SendPropertyChanged("ESNStr");
					this.OnESNStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_HUNStr", DbType="NVarChar(50)")]
		public string HUNStr
		{
			get
			{
				return this._HUNStr;
			}
			set
			{
				if ((this._HUNStr != value))
				{
					this.OnHUNStrChanging(value);
					this.SendPropertyChanging();
					this._HUNStr = value;
					this.SendPropertyChanged("HUNStr");
					this.OnHUNStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ITAStr", DbType="NVarChar(50)")]
		public string ITAStr
		{
			get
			{
				return this._ITAStr;
			}
			set
			{
				if ((this._ITAStr != value))
				{
					this.OnITAStrChanging(value);
					this.SendPropertyChanging();
					this._ITAStr = value;
					this.SendPropertyChanged("ITAStr");
					this.OnITAStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_KORStr", DbType="NVarChar(50)")]
		public string KORStr
		{
			get
			{
				return this._KORStr;
			}
			set
			{
				if ((this._KORStr != value))
				{
					this.OnKORStrChanging(value);
					this.SendPropertyChanging();
					this._KORStr = value;
					this.SendPropertyChanged("KORStr");
					this.OnKORStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_NLDStr", DbType="NVarChar(50)")]
		public string NLDStr
		{
			get
			{
				return this._NLDStr;
			}
			set
			{
				if ((this._NLDStr != value))
				{
					this.OnNLDStrChanging(value);
					this.SendPropertyChanging();
					this._NLDStr = value;
					this.SendPropertyChanged("NLDStr");
					this.OnNLDStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PLKStr", DbType="NVarChar(50)")]
		public string PLKStr
		{
			get
			{
				return this._PLKStr;
			}
			set
			{
				if ((this._PLKStr != value))
				{
					this.OnPLKStrChanging(value);
					this.SendPropertyChanging();
					this._PLKStr = value;
					this.SendPropertyChanged("PLKStr");
					this.OnPLKStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PTBStr", DbType="NVarChar(50)")]
		public string PTBStr
		{
			get
			{
				return this._PTBStr;
			}
			set
			{
				if ((this._PTBStr != value))
				{
					this.OnPTBStrChanging(value);
					this.SendPropertyChanging();
					this._PTBStr = value;
					this.SendPropertyChanged("PTBStr");
					this.OnPTBStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PTGStr", DbType="NVarChar(50)")]
		public string PTGStr
		{
			get
			{
				return this._PTGStr;
			}
			set
			{
				if ((this._PTGStr != value))
				{
					this.OnPTGStrChanging(value);
					this.SendPropertyChanging();
					this._PTGStr = value;
					this.SendPropertyChanged("PTGStr");
					this.OnPTGStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SVEStr", DbType="NVarChar(50)")]
		public string SVEStr
		{
			get
			{
				return this._SVEStr;
			}
			set
			{
				if ((this._SVEStr != value))
				{
					this.OnSVEStrChanging(value);
					this.SendPropertyChanging();
					this._SVEStr = value;
					this.SendPropertyChanged("SVEStr");
					this.OnSVEStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_TRKStr", DbType="NVarChar(50)")]
		public string TRKStr
		{
			get
			{
				return this._TRKStr;
			}
			set
			{
				if ((this._TRKStr != value))
				{
					this.OnTRKStrChanging(value);
					this.SendPropertyChanging();
					this._TRKStr = value;
					this.SendPropertyChanged("TRKStr");
					this.OnTRKStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DANStr", DbType="NVarChar(50)")]
		public string DANStr
		{
			get
			{
				return this._DANStr;
			}
			set
			{
				if ((this._DANStr != value))
				{
					this.OnDANStrChanging(value);
					this.SendPropertyChanging();
					this._DANStr = value;
					this.SendPropertyChanged("DANStr");
					this.OnDANStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ELLStr", DbType="NVarChar(50)")]
		public string ELLStr
		{
			get
			{
				return this._ELLStr;
			}
			set
			{
				if ((this._ELLStr != value))
				{
					this.OnELLStrChanging(value);
					this.SendPropertyChanging();
					this._ELLStr = value;
					this.SendPropertyChanged("ELLStr");
					this.OnELLStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_FINStr", DbType="NVarChar(50)")]
		public string FINStr
		{
			get
			{
				return this._FINStr;
			}
			set
			{
				if ((this._FINStr != value))
				{
					this.OnFINStrChanging(value);
					this.SendPropertyChanging();
					this._FINStr = value;
					this.SendPropertyChanged("FINStr");
					this.OnFINStrChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_NORStr", DbType="NVarChar(50)")]
		public string NORStr
		{
			get
			{
				return this._NORStr;
			}
			set
			{
				if ((this._NORStr != value))
				{
					this.OnNORStrChanging(value);
					this.SendPropertyChanging();
					this._NORStr = value;
					this.SendPropertyChanged("NORStr");
					this.OnNORStrChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
#pragma warning restore 1591