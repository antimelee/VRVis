# VRVis
# Author: Wei Wei

A plugin for visualizing 3d bar charts in VR. It creates immersive visualizations (only bar chart for now) automatically.
To use the plugin in unity: 
0). Make sure you have a csv file that stores your data, which you want to visualize. The csv file should have multiple rows with row name.
1). Download the VRBarchart.dll file. 
2). Put the .dll file under the "Plugin" folder of your unity project. 
3). Create an empty gameobject and attach a script for it. 
4). In the script, using VRVis. 

# Example code:
using UnityEngine;
using VRVis;

public class Bars : MonoBehaviour
{
    void Start()
    {
        BarChart example = new BarChart();
        example.CreateBarChart("Csv/test2", gameObject);
    }

}
