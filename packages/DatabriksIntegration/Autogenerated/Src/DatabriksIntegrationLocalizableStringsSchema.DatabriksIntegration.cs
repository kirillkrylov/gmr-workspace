namespace Terrasoft.Configuration
{

	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Globalization;
	using Terrasoft.Common;
	using Terrasoft.Core;
	using Terrasoft.Core.Configuration;

	#region Class: DatabriksIntegrationLocalizableStringsSchema

	/// <exclude/>
	public class DatabriksIntegrationLocalizableStringsSchema : Terrasoft.Core.SourceCodeSchema
	{

		#region Constructors: Public

		public DatabriksIntegrationLocalizableStringsSchema(SourceCodeSchemaManager sourceCodeSchemaManager)
			: base(sourceCodeSchemaManager) {
		}

		public DatabriksIntegrationLocalizableStringsSchema(DatabriksIntegrationLocalizableStringsSchema source)
			: base( source) {
		}

		#endregion

		#region Methods: Protected

		protected override void InitializeProperties() {
			base.InitializeProperties();
			UId = new Guid("6f4bb4b4-299d-4710-8512-281b4ae5dd1a");
			Name = "DatabriksIntegrationLocalizableStrings";
			ParentSchemaUId = new Guid("50e3acc0-26fc-4237-a095-849a1d534bd3");
			CreatedInPackageId = new Guid("82e0d32b-db64-4c48-859b-3d7d72a01755");
			ZipBody = new byte[] { 31,139,8,0,0,0,0,0,0,10,109,143,81,106,196,48,12,68,191,99,240,29,116,129,110,14,208,82,40,244,167,80,232,194,158,64,113,68,108,98,203,65,146,19,210,101,239,94,186,52,180,31,253,28,102,152,153,215,52,241,4,151,93,141,202,163,119,222,49,22,210,5,3,193,43,26,14,146,102,125,99,163,73,208,82,229,151,101,241,238,250,157,235,250,190,135,39,109,165,160,236,207,63,250,99,99,133,5,195,140,19,61,100,90,41,195,128,97,38,30,33,215,128,57,125,226,144,9,86,204,141,20,44,162,65,196,149,128,43,148,42,4,140,214,4,51,104,136,84,16,234,198,36,39,56,227,68,128,60,66,181,72,114,152,66,90,155,4,82,80,195,29,182,100,17,44,210,97,223,203,133,120,36,81,168,2,161,178,182,114,95,165,114,58,254,247,127,0,150,54,228,20,32,100,84,253,23,254,253,23,225,98,146,120,82,239,186,171,119,221,205,187,155,119,95,165,192,140,94,75,1,0,0 };
		}

		#endregion

		#region Methods: Public

		public override void GetParentRealUIds(Collection<Guid> realUIds) {
			base.GetParentRealUIds(realUIds);
			realUIds.Add(new Guid("6f4bb4b4-299d-4710-8512-281b4ae5dd1a"));
		}

		#endregion

	}

	#endregion

}

