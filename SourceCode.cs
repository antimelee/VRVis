using System;
using System.Data;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace VRVis
{
    /// <summary>
    /// Create a barchart based on data in csv format
    /// <summary>
    public class BarChart
    {
        //Make sure the csv file has a header row and a header column, even their value are whitespace.
        // Start is called before the first frame update
        private string[] XLabels;
        private string[] YLabels;
        //The width of each bar, default value: 0.1
        private float BarWidth;
        //The Length of each bar, default value: 0.1
        private float BarLength;
        //The gap between each bar, in x dimension, default value: 0.05
        private float XBarGap;
        //The gap between each bar, in y dimension, default value: 0.05
        private float YBarGap;
        //The height of base, default value: 0.1
        private float BaseHeight;

        /*
         * Global variables that users cannot manipulate
         */
        //The width of base
        private float BaseWidth;
        //The length of base
        private float BaseLength;
        private float OriginX;
        private float OriginY;
        private float OriginZ;
        private float FontWidth = 2f;
        //The max value of dataset
        private float MaxValue;
        //the scale of barchart, it's also the max height of bar
        private float Scale;
        private string DefaultLabelName = " ";
        private List<GameObject> XLabelObjects = new List<GameObject>();
        private List<GameObject> YLabelObjects = new List<GameObject>();

        public void CreateBarChart(string csvFilePath, GameObject parent, float scale = 1f, float barWidth = 0.1f, float barLength = 0.1f, float xBarGap = 0.05f, float yBarGap = 0.05f, float baseHeight = 0.1f)
        {
            Scale = scale;
            BarWidth = barWidth * Scale;
            BarLength = barLength * Scale;
            XBarGap = xBarGap * Scale;
            YBarGap = yBarGap * Scale;
            BaseHeight = baseHeight * Scale;
            DataTable visData = OpenCsv(csvFilePath);
            GameObject barBase = CreateBase(parent.transform, visData);
            CreateBars(parent, visData);
            WriteLabels(parent);
            CreateTicks(parent);
        }
        /*
        * Read data from csv file
        */
        public DataTable OpenCsv(string filePath)
        {
            DataTable dt = new DataTable();
            string strLine = "";
            string[] tableHead = null;
            int columnCount = 0;
            bool IsFirst = true;
            var dataset = Resources.Load<TextAsset>(filePath);
            // Splitting the dataset in the end of line
            var splitDataset = dataset.text.Split('\n');
            for (var i = 0; i < splitDataset.Length; i++)
            {
                string[] row = splitDataset[i].Split(',');
                if (IsFirst == true)
                {
                    tableHead = row;
                    IsFirst = false;
                    columnCount = tableHead.Length;
                    for (int key = 0; key < columnCount; key++)
                    {
                        // If column label is null, set it to default value: " "
                        if (tableHead[key] == "")
                            tableHead[key] = DefaultLabelName;
                        DataColumn dc = new DataColumn(tableHead[key]);
                        dt.Columns.Add(dc);
                    }
                }
                else
                {
                    DataRow dr = dt.NewRow();
                    for (int m = 0; m < row.Length; m++)
                    {
                        dr[m] = row[m];
                    }
                    dt.Rows.Add(dr);
                }
            }

            XLabels = tableHead;
            return dt;
        }

        /*
        * Create the base of bar chart
        */
        public GameObject CreateBase(Transform parent, DataTable visData)
        {
            int yBarNum = XLabels.Length - 1;
            int xBarNum = visData.Rows.Count;
            BaseWidth = (xBarNum) * BarWidth + (xBarNum - 1) * XBarGap;
            BaseLength = (yBarNum) * BarLength + (yBarNum - 1) * YBarGap;
            GameObject barBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barBase.name = "Base";
            barBase.transform.parent = parent;
            barBase.transform.localScale = new Vector3(BaseWidth, BaseHeight, BaseLength);
            barBase.transform.localPosition = new Vector3(0f, 0f, 0f);//shuffle the center point by half x,y,z.
            return barBase;
        }
        /*
        * Create bar chart
        */
        public List<List<GameObject>> CreateBars(GameObject aParent, DataTable visData)
        {

            List<List<GameObject>> bars = new List<List<GameObject>>();
            List<List<float>> data = new List<List<float>>();
            float maxValue = float.MinValue;
            YLabels = new string[visData.Rows.Count];
            for (int i = 0; i < visData.Rows.Count; i++)
            {
                List<float> row = new List<float>();
                YLabels[i] = visData.Rows[i][XLabels[0]].ToString();
                for (int j = 1; j < XLabels.Length; j++)
                {
                    float value;
                    if (!float.TryParse(visData.Rows[i][XLabels[j]].ToString(), out value))
                        //Need to be improved here
                        value = 0;
                    row.Add(value);
                    maxValue = maxValue > value ? maxValue : value;
                }
                data.Add(row);
            }
            //set MaxValue
            MaxValue = maxValue;
            //get Palette
            List<Color> barColors = CreatePalette();

            //convert the ordinate system from base center to bottom left of bar chart. 
            OriginX = -BaseWidth / 2;
            OriginZ = -BaseLength / 2;
            OriginY = BaseHeight / 2;
            int rowCount = 0;
            int columnCount = 0;
            foreach (List<float> row in data)
            {
                List<GameObject> rowObjects = new List<GameObject>();
                columnCount = 0;
                foreach (float val in row)
                {
                    float height = Scale * val / maxValue;
                    float xPos = rowCount * BarWidth + rowCount * XBarGap + BarWidth / 2 + OriginX;
                    float zPos = columnCount * BarLength + columnCount * YBarGap + BarLength / 2 + OriginZ;
                    float yPos = height / 2 + OriginY;
                    GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    var cubeRenderer = bar.GetComponent<Renderer>();
                    //cube.name = LikertNames_comfort[i];
                    //cube.tag = "Bar";
                    // cube.AddComponent<BarCollision>();
                    //loop colors
                    cubeRenderer.material.SetColor("_Color", barColors[(columnCount) % (barColors.Count)]);
                    bar.transform.parent = aParent.transform;
                    bar.transform.localScale = new Vector3(BarWidth, height, BarLength);
                    bar.transform.localPosition = new Vector3(xPos, yPos, zPos);
                    columnCount++;
                }
                rowCount++;
            }
            return bars;
        }

        /*
         * Write labels for three dimensions
         * Default text color: black;
         * Default text fontsize: 1
         */
        public void WriteLabels(GameObject aParent)
        {
            // Create the Text GameObject.

            // Label text color
            Color LabelTextColor = Color.black;
            // Label text font size, it changes according to the barchart scale (Scale)
            int DefaultLabelFontSize = 1;
            float labelOffset = -0.05f * Scale;

            // write x labels
            for (int i = 1; i < XLabels.Length; i++)
            {
                GameObject labelInstance = new GameObject();
                labelInstance.transform.parent = aParent.transform;
                labelInstance.AddComponent(typeof(TextMeshPro));
                // Set Text component properties.
                var labelText = labelInstance.GetComponent<TextMeshPro>();
                labelText.text = XLabels[i];
                // Set default parameters
                labelText.fontSize = DefaultLabelFontSize * Scale;
                //labelText.color = LabelTextColor;
                labelText.autoSizeTextContainer = true;
                labelInstance.transform.Rotate(90.0f, 0.0f, 0.0f);

                Vector2 textSize = labelText.GetPreferredValues();
                labelInstance.transform.localPosition = new Vector3(labelOffset + OriginX - (textSize.x) / 2, OriginY, (i - 1) * BarWidth + (i - 1) * XBarGap + BarWidth / 2 + OriginZ);
                XLabelObjects.Add(labelInstance);
            }

            // write y labels
            for (int i = 0; i < YLabels.Length; i++)
            {
                GameObject labelInstance = new GameObject();
                labelInstance.transform.parent = aParent.transform;
                labelInstance.AddComponent(typeof(TextMeshPro));
                // Set Text component properties.
                var labelText = labelInstance.GetComponent<TextMeshPro>();
                labelText.text = YLabels[i];
                // Set default parameters
                labelText.fontSize = DefaultLabelFontSize * Scale;
                //labelText.color = LabelTextColor;
                labelText.autoSizeTextContainer = true;
                labelInstance.transform.Rotate(90.0f, 90.0f, 0.0f);
                Vector2 textSize = labelText.GetPreferredValues();
                labelInstance.transform.localPosition = new Vector3(i * BarWidth + i * XBarGap + BarWidth / 2 + OriginX, OriginY, labelOffset + OriginZ - (textSize.x) / 2);
                YLabelObjects.Add(labelInstance);
            }
        }

        public void CreateTicks(GameObject aParent)
        {
            /* 
             * tick strategy: Normalize the maxValue to the range(1- 99), than choose accordingly:
            * 1-4: 0.5; 5-10: 1; 11-20: 2; 21-50: 5; 51-99: 10 
            */
            int topThreshHold = 100, bottomThreshHold = 1;
            float normalizeScale = 1;
            float normalizedMax = MaxValue;
            // Tick text font size, it changes according to the barchart scale, default value: 1
            int DefaultTickFontSize = 1;
            float DefaultTickLineWidth = 0.01f;
            while (normalizedMax > topThreshHold)
            {
                normalizedMax /= 10;
                normalizeScale *= 10;
            }
            while (normalizedMax < bottomThreshHold)
            {
                normalizedMax *= 10;
                normalizeScale /= 10;
            }
            float tickScale = 0.5f;
            if (normalizedMax > 4 && normalizedMax < 11)
                tickScale = 1;
            else if (normalizedMax > 10 && normalizedMax < 21)
                tickScale = 2;
            else if (normalizedMax > 20 && normalizedMax < 51)
                tickScale = 5;
            else tickScale = 10;
            int numOfTick = (int)Math.Ceiling(normalizedMax / tickScale);
            float heightOfOneTick = Scale * tickScale * normalizeScale / MaxValue;
            while (numOfTick > 0)
            {
                GameObject tickLineInstance = new GameObject();
                // draw the line of tick
                tickLineInstance.transform.parent = aParent.transform;
                tickLineInstance.AddComponent(typeof(LineRenderer));
                var tick = tickLineInstance.GetComponent<LineRenderer>();
                tick.positionCount = 3;
                tick.SetPosition(0, new Vector3(OriginX, OriginY + heightOfOneTick * numOfTick, OriginZ + BaseLength));
                tick.SetPosition(1, new Vector3(OriginX + BaseWidth, OriginY + heightOfOneTick * numOfTick, OriginZ + BaseLength));
                tick.SetPosition(2, new Vector3(OriginX + BaseWidth, OriginY + heightOfOneTick * numOfTick, OriginZ));
                tick.SetWidth(DefaultTickLineWidth * Scale, DefaultTickLineWidth * Scale);
                //tick.startColor = Color.black;
                //tick.endColor = Color.black;
                // write the text of tick
                GameObject tickInstance = new GameObject();
                // draw the line of tick
                tickInstance.transform.parent = aParent.transform;
                tickInstance.AddComponent(typeof(TextMeshPro));
                // Set Text component properties.
                var tickText = tickInstance.GetComponent<TextMeshPro>();
                tickText.text = (numOfTick * tickScale * normalizeScale).ToString();
                // Set default parameters
                tickText.fontSize = DefaultTickFontSize * Scale;
                //labelText.color = LabelTextColor;
                tickText.autoSizeTextContainer = true;
                Vector2 textSize = tickText.GetPreferredValues();
                // Set the pos of text according to data and text size.
                tickText.transform.localPosition = new Vector3(OriginX - textSize.y, OriginY + heightOfOneTick * numOfTick, OriginZ + BaseLength);
                numOfTick--;
            }
        }


        /*
            * Set the color, fontsize of x labels.
            * Need to extend the function to personize font, isAutoSize and so on
            */
        public void SetXLabels(int size, Color textColor)
        {
            foreach (var label in XLabelObjects)
            {
                var labelText = label.GetComponent<TextMeshPro>();
                labelText.fontSize = size;
                // labelText.color = textColor;
            }
        }

        /*
         * Set the color, fontsize of y labels.
         * Need to extend the function to personize font, isAutoSize and so on
         */
        public void SetYLabels(int size, Color textColor)
        {
            foreach (var label in XLabelObjects)
            {
                var labelText = label.GetComponent<TextMeshPro>();
                labelText.fontSize = size;
                //labelText.color = textColor;
            }
        }

        public List<Color> CreatePalette()
        {
            int interval = 120;

            List<Color> colors = new List<Color>();
            for (int red = 0; red < 255; red += interval)
            {
                for (int green = 0; green < 255; green += interval)
                {
                    for (int blue = 0; blue < 255; blue += interval)
                    {
                        if (red > 150 | blue > 150 | green > 150) //to make sure color is not too dark
                        {
                            Color c = new Color(red / 255f, green / 255f, blue / 255f);
                            colors.Add(c);
                        }
                    }
                }
            }
            return colors;
        }
    }
}
