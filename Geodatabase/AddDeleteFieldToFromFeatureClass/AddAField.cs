/*

   Copyright 2016 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Mapping;

namespace AddDeleteFieldToFromFeatureClass
{
    internal class AddAField : Button
    {
        protected override async void OnClick()
        {
            try
            {
                BasicFeatureLayer layer = null;
                await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {
                    //find selected layer
                    if (MapView.Active.GetSelectedLayers().Count == 0)
                    {
                        MessageBox.Show("Select a feature class from the Content 'Table of Content' first.");
                        return;
                    }
                    layer = MapView.Active.GetSelectedLayers()[0] as BasicFeatureLayer;

                });
                if (layer == null)
                    MessageBox.Show("Unable to find a feature class at the first layer of the active map");
                else
                {
                    MessageBox.Show($@"{layer.Name} was found .... adding a new Field");
                    await
                        ExecuteAddFieldTool(layer, new KeyValuePair<string, string>("AddedField", "Alias Name Added Field"), "Text", 50);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private async Task<bool> ExecuteAddFieldTool(BasicFeatureLayer theLayer, KeyValuePair<string, string> field, string fieldType, int? fieldLength = null, bool isNullable = true)
        {
            try
            {
                return await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {
                    var inTable = theLayer.Name;
                    var table = theLayer.GetTable();
                    var dataStore = table.GetDatastore();
                    var workspaceNameDef = dataStore.GetConnectionString();
                    var workspaceName = workspaceNameDef.Split('=')[1];
                    
                    var fullSpec = System.IO.Path.Combine(workspaceName, inTable);
                    System.Diagnostics.Debug.WriteLine($@"Add {field.Key} from {fullSpec}");

                    var parameters = Geoprocessing.MakeValueArray(inTable, field.Key, fieldType.ToUpper(), null, null,
                        fieldLength, field.Value, isNullable ? "NULABLE" : "NON_NULLABLE");
                    var env = Geoprocessing.MakeEnvironmentArray(workspace: workspaceName);
                    var cts = new CancellationTokenSource();
                    var results = Geoprocessing.ExecuteToolAsync("management.AddField", parameters, env, cts.Token,
                        (eventName, o) =>
                        {
                            System.Diagnostics.Debug.WriteLine($@"GP event: {eventName}");
                        });
                    return true;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }

    }
}
