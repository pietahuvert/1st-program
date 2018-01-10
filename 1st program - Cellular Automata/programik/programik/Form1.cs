using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace programik
{
    public partial class Form1 : Form
    {

        public int[,] globalTab;
        public int nucleonAmountGlobal;
        public List<int> exceptions = new List<int>();
        public List<Color> nucleonsLastColors = new List<Color>();

        public struct Coordinates
        {
            public int x;
            public int y;
        }
        
        public Form1()
        {
            InitializeComponent();
            bordersButton.Enabled = false;

            secondSimulationButton.Enabled = false;
            Bitmap DrawArea = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            pictureBox1.Image = DrawArea;

        }

        private void simulationButton_Click(object sender, EventArgs e)
        {
            StartSimulation();
        }



        private void StartSimulation()
        {
            
            int sizeX = int.Parse(textBoxSizeX.Text);
            int sizeY = int.Parse(textBoxSizeY.Text);
            int probability = int.Parse(probabilityTextBox.Text);
            int nucleonAmount = (int)nucleonAmountNumericUpDown.Value;
            nucleonAmountGlobal = nucleonAmount;
            int inclusionsAmount = (int)inclusionsNumeric.Value;
            int inclusionsRadius = (int)radiusNumeric.Value;
            bool canGrowth = true;
            int canGrowthCounter = 0;
            int lastCanGrowthCounter = 0;
            int errorCounter = 0;
            exceptions.Add(-1);
            exceptions.Add(-2);


            Bitmap bmp = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            pictureBox1.Image = bmp;

            //blokada maksymalnego rozmiaru macierzy
            if(sizeX > pictureBox1.Width)
            {
                sizeX = pictureBox1.Width;
                textBoxSizeX.Text = sizeX.ToString();
            }

            if (sizeY > pictureBox1.Height)
            {
                sizeY = pictureBox1.Height;
                textBoxSizeY.Text = sizeY.ToString();
            }

            //stworzenie macierzy aktualnej i z poprzedniego kroku
            int[,] tabActual = new int[sizeX, sizeY];
            int[,] tabLast = new int[sizeX, sizeY];

            for(int i = 0; i<sizeX; i++)
            {
                for(int j = 0; j<sizeY; j++)
                {
                    tabActual[i, j] = 0;
                    tabLast[i, j] = 0;
                }
            }

            Random random = new Random();

            //generate inclusions before
           if(inclusionsGenerationComboBox.SelectedItem.ToString().Equals("Before"))
            {
                GenerateInclusionsBefore(inclusionsAmount, random, sizeX, sizeY, tabActual, tabLast, inclusionsRadius);
            }

            for (int i = 0; i<nucleonAmount; i++)
            {
                int randomPositionX;
                int randomPositionY;
                do
                {
                    randomPositionX = random.Next(0, sizeX);
                    randomPositionY = random.Next(0, sizeY);
                }
                while (tabActual[randomPositionX, randomPositionY] != 0 && tabLast[randomPositionX, randomPositionY] != 0);

                tabActual[randomPositionX, randomPositionY] = i + 1;
                tabLast[randomPositionX, randomPositionY] = i + 1;

            }

            //colorise
            for(int i = 0; i<sizeX; i++)
            {
                for(int j = 0; j<sizeY; j++)
                {      
                     if(tabActual[i,j] == -1)
                    {
                        bmp.SetPixel(i, j, Color.Black);
                    }
                    
                }
            }

            pictureBox1.Refresh();

            

            List<Color> colors = new List<Color>();
            for(int i = 0; i<nucleonAmount; i++)
            {
                colors.Add(Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)));
                nucleonsLastColors.Add(colors[i]);
            }

            //growth main method
            while (canGrowth)
            {
                for (int i = 0; i < sizeX; i++)
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        //int tempValueLeft;
                        //int tempValueRight;
                        //int tempValueUp;
                        //int tempValueDown;

                        if (tabActual[i, j] == 0)
                        {
                            canGrowthCounter++;
                            if(!Moore(i, j, tabActual, tabLast, sizeX, sizeY, nucleonAmount))
                            {
                               if(!VonNeumann(i, j, tabActual, tabLast, sizeX, sizeY, nucleonAmount))
                               {
                                    if(!FurtherMoore(i, j, tabActual, tabLast, sizeX, sizeY, nucleonAmount))
                                    {
                                        Probability(i, j, tabActual, tabLast, sizeX, sizeY, nucleonAmount, random, probability);
                                        //MooreMax(i, j, tabActual, tabLast, sizeX, sizeY, nucleonAmount);
                                    }
                                }
                            }
                                
                                
                                

                            if (tabActual[i,j] > 0)
                            {
                                bmp.SetPixel(i, j, colors[tabActual[i, j] - 1]);
                            }
                            

                        }                        
                    }

                    
                }

                if(canGrowthCounter > 0)
                {
                    if(canGrowthCounter == lastCanGrowthCounter)
                    {
                        errorCounter++;
                        if(errorCounter > 50)
                        {
                            canGrowth = false;
                        }
                        canGrowthCounter = 0;
                    }
                    else
                    {
                        canGrowth = true;
                        lastCanGrowthCounter = canGrowthCounter;
                        canGrowthCounter = 0;
                        errorCounter = 0;
                    }
                    
                }
                else
                {
                    canGrowth = false;
                    canGrowthCounter = 0;
                }

                pictureBox1.Refresh();



                //rewrite
                for (int i = 0; i < sizeX; i++)
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        tabLast[i, j] = tabActual[i, j];
                    }
                }
            }

            //inclusions after
            if (inclusionsGenerationComboBox.SelectedItem.ToString().Equals("After"))
            {
                GenerateInclusionsAfter(inclusionsAmount, random, sizeX, sizeY, tabActual, tabLast, inclusionsRadius);
                for (int i = 0; i < sizeX; i++)
                {
                    for (int j = 0; j < sizeX; j++)
                    {
                        if (tabActual[i, j] == -1)
                        {
                            bmp.SetPixel(i, j, Color.Black);
                        }

                    }
                }

                pictureBox1.Refresh();
            }

            globalTab = tabActual;
            secondSimulationButton.Enabled = true;
            exceptions.Clear();
            bordersButton.Enabled = true;
        }

        private void DualPhase()
        {
            int sizeX = int.Parse(textBoxSizeX.Text);
            int sizeY = int.Parse(textBoxSizeY.Text);
            int probability = int.Parse(probabilityTextBox.Text);
            int nucleonAmount = (int)nucleonAmountNumericUpDown.Value;
            int dualPhaseNucleonAmount = (int)nucleonAmountDualPhaseNumeric.Value;
            int inclusionsAmount = (int)inclusionsNumeric.Value;
            int inclusionsRadius = (int)radiusNumeric.Value;
            bool canGrowth = true;
            int canGrowthCounter = 0;
            int lastCanGrowthCounter = 0;
            int errorCounter = 0;
            exceptions.Add(-1);
            exceptions.Add(-2);

            Bitmap bmp = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            pictureBox1.Image = bmp;

            //blokada maksymalnego rozmiaru macierzy
            if (sizeX > pictureBox1.Width)
            {
                sizeX = pictureBox1.Width;
                textBoxSizeX.Text = sizeX.ToString();
            }

            if (sizeY > pictureBox1.Height)
            {
                sizeY = pictureBox1.Height;
                textBoxSizeY.Text = sizeY.ToString();
            }

            Random random = new Random();
            List<int> nucleonsToRewrite = new List<int>();

            for(int i = 0; i<dualPhaseNucleonAmount; i++)
            {
                int randomValue = random.Next(1, nucleonAmountGlobal + 1);
                while(nucleonsToRewrite.Contains(randomValue))
                {
                    randomValue = random.Next(1, nucleonAmountGlobal + 1);
                }
                nucleonsToRewrite.Add(randomValue);
            }
            nucleonsToRewrite.Add(-1);

            //stworzenie macierzy aktualnej i z poprzedniego kroku
            int[,] tabActual = new int[sizeX, sizeY];
            int[,] tabLast = new int[sizeX, sizeY];

            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    for(int x = 0; x<nucleonsToRewrite.Count(); x++)
                    {
                        if(structureComboBox.SelectedItem.ToString().Equals("SUB"))
                        {
                            if (nucleonsToRewrite[x] == globalTab[i, j])
                            {
                                if (nucleonsToRewrite[x] == -1)
                                {
                                    tabActual[i, j] = -1;
                                    tabLast[i, j] = -1;
                                }
                                else
                                {
                                    tabActual[i, j] = -2;
                                    tabLast[i, j] = -2;
                                }
                                break;
                            }
                            else
                            {
                                tabActual[i, j] = 0;
                                tabLast[i, j] = 0;
                            }
                        }

                        if (structureComboBox.SelectedItem.ToString().Equals("DP"))
                        {
                            if (nucleonsToRewrite[x] == globalTab[i, j])
                            {
                                if (nucleonsToRewrite[x] == -1)
                                {
                                    tabActual[i, j] = -1;
                                    tabLast[i, j] = -1;
                                }
                                else
                                {
                                    tabActual[i, j] = nucleonsToRewrite[x];
                                    tabLast[i, j] = nucleonsToRewrite[x];
                                    exceptions.Add(nucleonsToRewrite[x]);
                                }
                                break;
                            }
                            else
                            {
                                tabActual[i, j] = 0;
                                tabLast[i, j] = 0;
                            }
                        }
                    }
                }
            }

            

            //generate inclusions before
            if (inclusionsGenerationComboBox.SelectedItem.ToString().Equals("Before"))
            {
                GenerateInclusionsBefore(inclusionsAmount, random, sizeX, sizeY, tabActual, tabLast, inclusionsRadius);
            }

            for (int i = 0; i < nucleonAmount; i++)
            {
                int randomPositionX;
                int randomPositionY;
                do
                {
                    randomPositionX = random.Next(0, sizeX);
                    randomPositionY = random.Next(0, sizeY);
                }
                while (tabActual[randomPositionX, randomPositionY] != 0 && tabLast[randomPositionX, randomPositionY] != 0);

                tabActual[randomPositionX, randomPositionY] = i + 1;
                tabLast[randomPositionX, randomPositionY] = i + 1;

            }

            //colorise
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    if (tabActual[i, j] == -1)
                    {
                        bmp.SetPixel(i, j, Color.Black);
                    }
                    else if (tabActual[i, j] == -2)
                    {
                        bmp.SetPixel(i, j, Color.Pink);
                    }
                    else if(nucleonsToRewrite.Contains(tabActual[i,j]))
                    {
                        bmp.SetPixel(i, j, nucleonsLastColors[tabActual[i, j] - 1]);
                    }

                }
            }
            nucleonsLastColors.Clear();

            pictureBox1.Refresh();



            List<Color> colors = new List<Color>();
            for (int i = 0; i < nucleonAmount; i++)
            {
                colors.Add(Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)));
                //nucleonsLastColors.Add(colors[i]);
            }

            //growth main method
            while (canGrowth)
            {
                for (int i = 0; i < sizeX; i++)
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        //int tempValueLeft;
                        //int tempValueRight;
                        //int tempValueUp;
                        //int tempValueDown;

                        if (tabActual[i, j] == 0)
                        {
                            canGrowthCounter++;
                            if (!Moore(i, j, tabActual, tabLast, sizeX, sizeY, nucleonAmount))
                            {
                                if (!VonNeumann(i, j, tabActual, tabLast, sizeX, sizeY, nucleonAmount))
                                {
                                    if (!FurtherMoore(i, j, tabActual, tabLast, sizeX, sizeY, nucleonAmount))
                                    {
                                        Probability(i, j, tabActual, tabLast, sizeX, sizeY, nucleonAmount, random, probability);
                                        //MooreMax(i, j, tabActual, tabLast, sizeX, sizeY, nucleonAmount);
                                    }
                                }
                            }




                            if (tabActual[i, j] > 0)
                            {
                                bmp.SetPixel(i, j, colors[tabActual[i, j] - 1]);
                            }


                        }
                    }


                }

                if (canGrowthCounter > 0)
                {
                    if (canGrowthCounter == lastCanGrowthCounter)
                    {
                        errorCounter++;
                        if (errorCounter > 50)
                        {
                            canGrowth = false;
                        }
                        canGrowthCounter = 0;
                    }
                    else
                    {
                        canGrowth = true;
                        lastCanGrowthCounter = canGrowthCounter;
                        canGrowthCounter = 0;
                        errorCounter = 0;
                    }

                }
                else
                {
                    canGrowth = false;
                    canGrowthCounter = 0;
                }

                pictureBox1.Refresh();



                //rewrite
                for (int i = 0; i < sizeX; i++)
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        tabLast[i, j] = tabActual[i, j];
                    }
                }
            }
            
            //inclusions after
            if (inclusionsGenerationComboBox.SelectedItem.ToString().Equals("After"))
            {
                GenerateInclusionsAfter(inclusionsAmount, random, sizeX, sizeY, tabActual, tabLast, inclusionsRadius);
                for (int i = 0; i < sizeX; i++)
                {
                    for (int j = 0; j < sizeX; j++)
                    {
                        if (tabActual[i, j] == -1)
                        {
                            bmp.SetPixel(i, j, Color.Black);
                        }

                    }
                }

                pictureBox1.Refresh();
            }

            globalTab = tabActual;
            nucleonsToRewrite.Clear();
            simulationButton.Enabled = false;
            secondSimulationButton.Enabled = false;
            bordersButton.Enabled = true;

        }

        private void SelectBorders()
        {
            int sizeX = int.Parse(textBoxSizeX.Text);
            int sizeY = int.Parse(textBoxSizeY.Text);

            Bitmap bmp = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            pictureBox1.Image = bmp;

            int borderSize = (int)borderSizeNumericUpDown.Value;
            int grainAmount = (int)grainAmountumericUpDown.Value;

            Random random = new Random();
            List<int> selectedNucleons = new List<int>();

            for (int i = 0; i < grainAmount; i++)
            {
                int randomValue = random.Next(1, nucleonAmountGlobal + 1);
                while (selectedNucleons.Contains(randomValue))
                {
                    randomValue = random.Next(1, nucleonAmountGlobal + 1);
                }
                selectedNucleons.Add(randomValue);
            }
            selectedNucleons.Add(-1);
            

            int[,] tabActual = new int[sizeX, sizeY];
            int[,] tabLast = new int[sizeX, sizeY];
            int[,] tabBorders = new int[sizeX, sizeY];
            int[,] tabLastBorders = new int[sizeX, sizeY];

            for (int i = 0; i<sizeX; i++)
            {
                for(int j = 0; j<sizeY; j++)
                {
                    tabActual[i, j] = globalTab[i, j];
                    tabLast[i, j] = globalTab[i, j];
                    tabBorders[i, j] = 0;
                    tabLastBorders[i, j] = 0;
                }
            }


            if (borderTypeComboBox.SelectedItem.ToString().Equals("FULL"))
            {
                for (int i = 0; i < sizeX; i++) //first iteration
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        BorderCheckup(i, j, tabActual, tabLast, tabBorders, sizeX, sizeY);
                    }
                }

                for (int i = 0; i < sizeX; i++) //first rewrite
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        tabLastBorders[i, j] = tabBorders[i, j];
                    }
                }
                

                for(int x = 0; x < borderSize - 1; x++) //next iterations
                {
                    for (int i = 0; i < sizeX; i++)
                    {
                        for (int j = 0; j < sizeY; j++)
                        {
                            BorderCheckup(i, j, tabBorders, tabLastBorders, tabBorders, sizeX, sizeY);
                        }
                    }

                    for (int i = 0; i < sizeX; i++) //rewrite
                    {
                        for (int j = 0; j < sizeY; j++)
                        {
                            tabLastBorders[i, j] = tabBorders[i, j];
                        }
                    }
                }

                for (int i = 0; i < sizeX; i++)
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        if (tabBorders[i, j] == -1)
                        {
                            bmp.SetPixel(i, j, Color.Black);
                        }
                    }
                }

                pictureBox1.Refresh();
                
            }

            if(borderTypeComboBox.SelectedItem.ToString().Equals("SELECTED"))
            {
                for (int i = 0; i < sizeX; i++) //first iteration
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        BorderSelectedCheckup(i, j, tabActual, tabLast, tabBorders, sizeX, sizeY, selectedNucleons);
                    }
                }

                for (int i = 0; i < sizeX; i++) //first rewrite
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        tabLastBorders[i, j] = tabBorders[i, j];
                    }
                }


                for (int x = 0; x < borderSize - 1; x++) //next iterations
                {
                    for (int i = 0; i < sizeX; i++)
                    {
                        for (int j = 0; j < sizeY; j++)
                        {
                            BorderCheckup(i, j, tabBorders, tabLastBorders, tabBorders, sizeX, sizeY);
                        }
                    }

                    for (int i = 0; i < sizeX; i++) //rewrite
                    {
                        for (int j = 0; j < sizeY; j++)
                        {
                            tabLastBorders[i, j] = tabBorders[i, j];
                        }
                    }
                }

                for (int i = 0; i < sizeX; i++)
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        if (tabBorders[i, j] == -1)
                        {
                            bmp.SetPixel(i, j, Color.Black);
                        }
                    }
                }

                pictureBox1.Refresh();
            }



        }


        private void GenerateInclusionsBefore(int inclusionsAmount, Random random, int sizeX, int sizeY, int[,] tabActual, int[,] tabLast, int inclusionsRadius)
        {
            for (int i = 0; i < inclusionsAmount; i++)
            {
                int randomPositionX = random.Next(0, sizeX);
                int randomPositionY = random.Next(0, sizeY);

                tabActual[randomPositionX, randomPositionY] = -1;
                tabLast[randomPositionX, randomPositionY] = -1;

                if (TypeOfInclusion.SelectedItem.ToString().Equals("Radial"))
                {
                    for (int x = (-1) * inclusionsRadius; x <= inclusionsRadius; x++)
                    {
                        for (int y = (-1) * inclusionsRadius; y <= inclusionsRadius; y++)
                        {
                            int rx = randomPositionX + x;
                            int ry = randomPositionY + y;
                            if (randomPositionX + x >= 0 && randomPositionX + x < sizeX && randomPositionY + y >= 0 && randomPositionY + y < sizeY)
                            {
                                if (Math.Sqrt(x * x + y * y) <= inclusionsRadius)
                                {
                                    tabActual[rx, ry] = -1;
                                    tabLast[rx, ry] = -1;
                                }
                            }
                        }
                    }
                }

                if (TypeOfInclusion.SelectedItem.ToString().Equals("Square"))
                {
                    for (int x = (-1) * inclusionsRadius; x <= inclusionsRadius; x++)
                    {
                        for (int y = (-1) * inclusionsRadius; y <= inclusionsRadius; y++)
                        {
                            int ax = randomPositionX + x;
                            int ay = randomPositionY + y;
                            if (randomPositionX + x >= 0 && randomPositionX + x < sizeX && randomPositionY + y >= 0 && randomPositionY + y < sizeY)
                            {
                                tabActual[ax, ay] = -1;
                                tabLast[ax, ay] = -1;
                            }
                        }
                    }
                }


            }
        }

        private void GenerateInclusionsAfter(int inclusionsAmount, Random random, int sizeX, int sizeY, int[,] tabActual, int[,] tabLast, int inclusionsRadius)
        {
            List<Coordinates> borderPositions = new List<Coordinates>();
            Coordinates tempBorderPosition = new Coordinates();

            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    //check left
                    if (i != 0)
                    {
                        if (tabLast[i - 1, j] != tabLast[i,j])
                        {
                            tempBorderPosition.x = i-1;
                            tempBorderPosition.y = j;
                            borderPositions.Add(tempBorderPosition);
                        }
                    }
                    //check right
                    if (i != sizeX - 1)
                    {
                        if (tabLast[i + 1, j] != tabLast[i, j])
                        {
                            tempBorderPosition.x = i + 1;
                            tempBorderPosition.y = j;
                            borderPositions.Add(tempBorderPosition);
                        }
                    }
                    //check up
                    if (j != 0)
                    {
                        if (tabLast[i, j - 1] != tabLast[i, j])
                        {
                            tempBorderPosition.x = i;
                            tempBorderPosition.y = j - 1;
                            borderPositions.Add(tempBorderPosition);
                        }
                    }
                    //check down
                    if (j != sizeY - 1)
                    {
                        if (tabLast[i, j + 1] != tabLast[i, j])
                        {
                            tempBorderPosition.x = i;
                            tempBorderPosition.y = j + 1;
                            borderPositions.Add(tempBorderPosition);
                        }
                    }
                }
            }

            int borderPositionsCount = borderPositions.Count;

            List<Coordinates> selectedBorderPositions = new List<Coordinates>();

            for(int i = 0; i<inclusionsAmount; i++)
            {
                selectedBorderPositions.Add(borderPositions[random.Next(0, borderPositionsCount)]);
            }

            for(int i = 0; i<inclusionsAmount; i++)
            {
                if (TypeOfInclusion.SelectedItem.ToString().Equals("Radial"))
                {
                    for (int x = (-1) * inclusionsRadius; x <= inclusionsRadius; x++)
                    {
                        for (int y = (-1) * inclusionsRadius; y <= inclusionsRadius; y++)
                        {
                            int rx = selectedBorderPositions[i].x + x;
                            int ry = selectedBorderPositions[i].y + y;
                            if (selectedBorderPositions[i].x + x >= 0 && selectedBorderPositions[i].x + x < sizeX && selectedBorderPositions[i].y + y >= 0 && selectedBorderPositions[i].y + y < sizeY)
                            {
                                if (Math.Sqrt(x * x + y * y) <= inclusionsRadius)
                                {
                                    tabActual[rx, ry] = -1;
                                    tabLast[rx, ry] = -1;
                                }
                            }
                        }
                    }
                }

                if (TypeOfInclusion.SelectedItem.ToString().Equals("Square"))
                {
                    for (int x = (-1) * inclusionsRadius; x <= inclusionsRadius; x++)
                    {
                        for (int y = (-1) * inclusionsRadius; y <= inclusionsRadius; y++)
                        {
                            int ax = selectedBorderPositions[i].x + x;
                            int ay = selectedBorderPositions[i].y + y;
                            if (selectedBorderPositions[i].x + x >= 0 && selectedBorderPositions[i].x + x < sizeX && selectedBorderPositions[i].y + y >= 0 && selectedBorderPositions[i].y + y < sizeY)
                            {
                                tabActual[ax, ay] = -1;
                                tabLast[ax, ay] = -1;
                            }
                        }
                    }
                }
            }



        }

        private bool VonNeumann(int i, int j, int[,] tabActual, int[,] tabLast, int sizeX, int sizeY, int nucleonsAmount)
        {
            List<int> neighbors = new List<int>();

            //check left
            if (i != 0)
            {
                if (tabLast[i - 1, j] != 0 && !exceptions.Contains(tabLast[i - 1, j]))  
                {                    
                    neighbors.Add(tabLast[i - 1, j]);
                }
            }
            //check right
            if (i != sizeX - 1)
            {
                if (tabLast[i + 1, j] != 0 && !exceptions.Contains(tabLast[i + 1, j]))
                {
                    neighbors.Add(tabLast[i + 1, j]);
                }
            }
            //check up
            if (j != 0)
            {
                if (tabLast[i, j - 1] != 0 && !exceptions.Contains(tabLast[i, j - 1]))
                {
                    neighbors.Add(tabLast[i, j - 1]);
                }
            }
            //check down
            if (j != sizeY - 1)
            {
                if (tabLast[i, j + 1] != 0 && !exceptions.Contains(tabLast[i, j + 1]))
                {
                    neighbors.Add(tabLast[i, j + 1]);
                }
            }

            //rule 2
            int counter = 0;
            if (neighbors.Count > 0)
            {
                for (int x = 0; x < nucleonsAmount; x++)
                {
                    foreach (var v in neighbors)
                    {
                        if (v == x + 1)
                        {
                            counter++;
                        }
                    }
                    if(counter >= 5)
                    {
                        tabActual[i, j] = x + 1;
                        counter = 0;
                        return true;
                    }
                    else
                    {
                        counter = 0;
                    }
                    
                }
            }
            return false;

            //if (neighbors.Count > 0)
            //{
            //    tabActual[i, j] = neighbors.Max();
            //}
        }
        private bool Moore(int i, int j, int[,] tabActual, int[,] tabLast, int sizeX, int sizeY, int nucleonsAmount)
        {
            List<int> neighbors = new List<int>();

            //check left
            if (i != 0)
            {
                if (tabLast[i - 1, j] != 0 && !exceptions.Contains(tabLast[i - 1, j]))
                {
                    neighbors.Add(tabLast[i - 1, j]);
                }
            }
            //check left up
            if(i != 0 && j != 0)
            {
                if (tabLast[i - 1, j - 1] != 0 && !exceptions.Contains(tabLast[i - 1, j - 1]))
                {
                    neighbors.Add(tabLast[i - 1, j - 1]);
                }
            }
            //check left down
            if (i != 0 && j != sizeY - 1)
            {
                if (tabLast[i - 1, j + 1] != 0 && !exceptions.Contains(tabLast[i - 1, j + 1]))
                {
                    neighbors.Add(tabLast[i - 1, j + 1]);
                }
            }
            //check right
            if (i != sizeX - 1)
            {
                if (tabLast[i + 1, j] != 0 && !exceptions.Contains(tabLast[i + 1, j]))
                {
                    neighbors.Add(tabLast[i + 1, j]);
                }
            }
            //check right up
            if (i != sizeX - 1 && j != 0)
            {
                if (tabLast[i + 1, j - 1] != 0 && !exceptions.Contains(tabLast[i + 1, j - 1]))
                {
                    neighbors.Add(tabLast[i + 1, j - 1]);
                }
            }
            //check right down
            if (i != sizeX - 1 && j != sizeY - 1)
            {
                if (tabLast[i + 1, j + 1] != 0 && !exceptions.Contains(tabLast[i + 1, j + 1]))
                {
                    neighbors.Add(tabLast[i + 1, j + 1]);
                }
            }
            //check up
            if (j != 0)
            {
                if (tabLast[i, j - 1] != 0 && !exceptions.Contains(tabLast[i, j - 1]))
                {
                    neighbors.Add(tabLast[i, j - 1]);
                }
            }
            //check down
            if (j != sizeY - 1)
            {
                if (tabLast[i, j + 1] != 0 && !exceptions.Contains(tabLast[i, j + 1]))
                {
                    neighbors.Add(tabLast[i, j + 1]);
                }
            }

            //rule 1
            int counter = 0;
            if (neighbors.Count > 0)
            {
                for (int x = 0; x < nucleonsAmount; x++)
                {
                    foreach (var v in neighbors)
                    {
                        if (v == x + 1)
                        {
                            counter++;
                        }
                    }
                    if (counter >= 3)
                    {
                        tabActual[i, j] = x + 1;
                        counter = 0;
                        return true;
                    }
                    else
                    {
                        counter = 0;
                    }

                }
            }
            return false;
            //if (neighbors.Count > 0)
            //{
            //    tabActual[i, j] = neighbors.Max();
            //}
        }
        private bool FurtherMoore(int i, int j, int[,] tabActual, int[,] tabLast, int sizeX, int sizeY, int nucleonsAmount)
        {
            List<int> neighbors = new List<int>();

            
            //check left up
            if (i != 0 && j != 0)
            {
                if (tabLast[i - 1, j - 1] != 0 && !exceptions.Contains(tabLast[i - 1, j - 1]))
                {
                    neighbors.Add(tabLast[i - 1, j - 1]);
                }
            }
            //check left down
            if (i != 0 && j != sizeY - 1)
            {
                if (tabLast[i - 1, j + 1] != 0 && !exceptions.Contains(tabLast[i - 1, j + 1]))
                {
                    neighbors.Add(tabLast[i - 1, j + 1]);
                }
            }            
            //check right up
            if (i != sizeX - 1 && j != 0)
            {
                if (tabLast[i + 1, j - 1] != 0 && !exceptions.Contains(tabLast[i + 1, j - 1]))
                {
                    neighbors.Add(tabLast[i + 1, j - 1]);
                }
            }
            //check right down
            if (i != sizeX - 1 && j != sizeY - 1)
            {
                if (tabLast[i + 1, j + 1] != 0 && !exceptions.Contains(tabLast[i + 1, j + 1]))
                {
                    neighbors.Add(tabLast[i + 1, j + 1]);
                }
            }

            //rule 3
            int counter = 0;
            if (neighbors.Count > 0)
            {
                for (int x = 0; x < nucleonsAmount; x++)
                {
                    foreach (var v in neighbors)
                    {
                        if (v == x + 1)
                        {
                            counter++;
                        }
                    }
                    if (counter >= 3)
                    {
                        tabActual[i, j] = x + 1;
                        counter = 0;
                        return true;
                    }
                    else
                    {
                        counter = 0;
                    }

                }
            }
            return false;
            //if (neighbors.Count > 0)
            //{
            //    tabActual[i, j] = neighbors.Max();
            //}
        }
        private void MooreMax(int i, int j, int[,] tabActual, int[,] tabLast, int sizeX, int sizeY, int nucleonsAmount)
        {
            List<int> neighbors = new List<int>();

            //check left
            if (i != 0)
            {
                if (tabLast[i - 1, j] != 0 && !exceptions.Contains(tabLast[i - 1, j]))
                {
                    neighbors.Add(tabLast[i - 1, j]);
                }
            }
            //check left up
            if (i != 0 && j != 0)
            {
                if (tabLast[i - 1, j - 1] != 0 && !exceptions.Contains(tabLast[i - 1, j - 1]))
                {
                    neighbors.Add(tabLast[i - 1, j - 1]);
                }
            }
            //check left down
            if (i != 0 && j != sizeY - 1)
            {
                if (tabLast[i - 1, j + 1] != 0 && !exceptions.Contains(tabLast[i - 1, j + 1]))
                {
                    neighbors.Add(tabLast[i - 1, j + 1]);
                }
            }
            //check right
            if (i != sizeX - 1)
            {
                if (tabLast[i + 1, j] != 0 && !exceptions.Contains(tabLast[i + 1, j]))
                {
                    neighbors.Add(tabLast[i + 1, j]);
                }
            }
            //check right up
            if (i != sizeX - 1 && j != 0)
            {
                if (tabLast[i + 1, j - 1] != 0 && !exceptions.Contains(tabLast[i + 1, j - 1]))
                {
                    neighbors.Add(tabLast[i + 1, j - 1]);
                }
            }
            //check right down
            if (i != sizeX - 1 && j != sizeY - 1)
            {
                if (tabLast[i + 1, j + 1] != 0 && !exceptions.Contains(tabLast[i + 1, j + 1]))
                {
                    neighbors.Add(tabLast[i + 1, j + 1]);
                }
            }
            //check up
            if (j != 0)
            {
                if (tabLast[i, j - 1] != 0 && !exceptions.Contains(tabLast[i, j - 1]))
                {
                    neighbors.Add(tabLast[i, j - 1]);
                }
            }
            //check down
            if (j != sizeY - 1)
            {
                if (tabLast[i, j + 1] != 0 && !exceptions.Contains(tabLast[i, j + 1]))
                {
                    neighbors.Add(tabLast[i, j + 1]);
                }
            }


            if (neighbors.Count > 0)
            {
                tabActual[i, j] = neighbors.Max();
            }
        }
        private void FurtherMooreMax(int i, int j, int[,] tabActual, int[,] tabLast, int sizeX, int sizeY, int nucleonsAmount)
        {
            List<int> neighbors = new List<int>();


            //check left up
            if (i != 0 && j != 0)
            {
                if (tabLast[i - 1, j - 1] != 0 && !exceptions.Contains(tabLast[i - 1, j - 1]))
                {
                    neighbors.Add(tabLast[i - 1, j - 1]);
                }
            }
            //check left down
            if (i != 0 && j != sizeY - 1)
            {
                if (tabLast[i - 1, j + 1] != 0 && !exceptions.Contains(tabLast[i - 1, j + 1]))
                {
                    neighbors.Add(tabLast[i - 1, j + 1]);
                }
            }
            //check right up
            if (i != sizeX - 1 && j != 0)
            {
                if (tabLast[i + 1, j - 1] != 0 && !exceptions.Contains(tabLast[i + 1, j - 1]))
                {
                    neighbors.Add(tabLast[i + 1, j - 1]);
                }
            }
            //check right down
            if (i != sizeX - 1 && j != sizeY - 1)
            {
                if (tabLast[i + 1, j + 1] != 0 && !exceptions.Contains(tabLast[i + 1, j + 1]))
                {
                    neighbors.Add(tabLast[i + 1, j + 1]);
                }
            }


            if (neighbors.Count > 0)
            {
                tabActual[i, j] = neighbors.Max();
            }
        }
        private void VonNeumannMax(int i, int j, int[,] tabActual, int[,] tabLast, int sizeX, int sizeY, int nucleonsAmount)
        {
            List<int> neighbors = new List<int>();

            //check left
            if (i != 0)
            {
                if (tabLast[i - 1, j] != 0 && !exceptions.Contains(tabLast[i - 1, j]))
                {
                    neighbors.Add(tabLast[i - 1, j]);
                }
            }
            //check right
            if (i != sizeX - 1)
            {
                if (tabLast[i + 1, j] != 0 && !exceptions.Contains(tabLast[i + 1, j]))
                {
                    neighbors.Add(tabLast[i + 1, j]);
                }
            }
            //check up
            if (j != 0)
            {
                if (tabLast[i, j - 1] != 0 && !exceptions.Contains(tabLast[i, j - 1]))
                {
                    neighbors.Add(tabLast[i, j - 1]);
                }
            }
            //check down
            if (j != sizeY - 1)
            {
                if (tabLast[i, j + 1] != 0 && !exceptions.Contains(tabLast[i, j + 1]))
                {
                    neighbors.Add(tabLast[i, j + 1]);
                }
            }

            if (neighbors.Count > 0)
            {
                tabActual[i, j] = neighbors.Max();
            }
        }
        private void Probability(int i, int j, int[,] tabActual, int[,] tabLast, int sizeX, int sizeY, int nucleonsAmount, Random random, int probability)
        {
            List<int> neighbors = new List<int>();            
            int randomNumber = 0;

            //check left
            if (i != 0)
            {
                if (tabLast[i - 1, j] != 0 && !exceptions.Contains(tabLast[i - 1, j]))
                {
                    neighbors.Add(tabLast[i - 1, j]);
                }
            }
            //check left up
            if (i != 0 && j != 0)
            {
                if (tabLast[i - 1, j - 1] != 0 && !exceptions.Contains(tabLast[i - 1, j - 1]))
                {
                    neighbors.Add(tabLast[i - 1, j - 1]);
                }
            }
            //check left down
            if (i != 0 && j != sizeY - 1)
            {
                if (tabLast[i - 1, j + 1] != 0 && !exceptions.Contains(tabLast[i - 1, j + 1]))
                {
                    neighbors.Add(tabLast[i - 1, j + 1]);
                }
            }
            //check right
            if (i != sizeX - 1)
            {
                if (tabLast[i + 1, j] != 0 && !exceptions.Contains(tabLast[i + 1, j]))
                {
                    neighbors.Add(tabLast[i + 1, j]);
                }
            }
            //check right up
            if (i != sizeX - 1 && j != 0)
            {
                if (tabLast[i + 1, j - 1] != 0 && !exceptions.Contains(tabLast[i + 1, j - 1]))
                {
                    neighbors.Add(tabLast[i + 1, j - 1]);
                }
            }
            //check right down
            if (i != sizeX - 1 && j != sizeY - 1)
            {
                if (tabLast[i + 1, j + 1] != 0 && !exceptions.Contains(tabLast[i + 1, j + 1]))
                {
                    neighbors.Add(tabLast[i + 1, j + 1]);
                }
            }
            //check up
            if (j != 0)
            {
                if (tabLast[i, j - 1] != 0 && !exceptions.Contains(tabLast[i, j - 1]))
                {
                    neighbors.Add(tabLast[i, j - 1]);
                }
            }
            //check down
            if (j != sizeY - 1)
            {
                if (tabLast[i, j + 1] != 0 && !exceptions.Contains(tabLast[i, j + 1]))
                {
                    neighbors.Add(tabLast[i, j + 1]);
                }
            }
            
            randomNumber = random.Next(0, 101);
            if(randomNumber <= probability)
            {
                if (neighbors.Count > 0)
                {
                    tabActual[i, j] = neighbors.Max();
                }
            }            
        }
        private void BorderCheckup(int i, int j, int[,] tabActual, int[,] tabLast, int[,] tabBorders, int sizeX, int sizeY)
        {
            
            //check left
            if (i != 0)
            {
                if (tabLast[i - 1, j] != tabActual[i,j])
                {
                    tabBorders[i, j] = -1;
                }
            }
            //check left up
            if (i != 0 && j != 0)
            {
                if (tabLast[i - 1, j - 1] != tabActual[i, j])
                {
                    tabBorders[i, j] = -1;
                }
            }
            //check left down
            if (i != 0 && j != sizeY - 1)
            {
                if (tabLast[i - 1, j + 1] != tabActual[i, j])
                {
                    tabBorders[i, j] = -1;
                }
            }
            //check right
            if (i != sizeX - 1)
            {
                if (tabLast[i + 1, j] != tabActual[i, j])
                {
                    tabBorders[i, j] = -1;
                }
            }
            //check right up
            if (i != sizeX - 1 && j != 0)
            {
                if (tabLast[i + 1, j - 1] != tabActual[i, j])
                {
                    tabBorders[i, j] = -1;
                }
            }
            //check right down
            if (i != sizeX - 1 && j != sizeY - 1)
            {
                if (tabLast[i + 1, j + 1] != tabActual[i, j])
                {
                    tabBorders[i, j] = -1;
                }
            }
            //check up
            if (j != 0)
            {
                if (tabLast[i, j - 1] != tabActual[i, j])
                {
                    tabBorders[i, j] = -1;
                }
            }
            //check down
            if (j != sizeY - 1)
            {
                if (tabLast[i, j + 1] != tabActual[i, j])
                {
                    tabBorders[i, j] = -1;
                }
            }

        }
        private void BorderSelectedCheckup(int i, int j, int[,] tabActual, int[,] tabLast, int[,] tabBorders, int sizeX, int sizeY, List<int> selectedNucleons)
        {
            //check left
            if (i != 0)
            {
                if (tabLast[i - 1, j] != tabActual[i, j] && selectedNucleons.Contains(tabActual[i, j]))
                {
                    tabBorders[i, j] = -1;
                }
            }
            //check left up
            if (i != 0 && j != 0)
            {
                if (tabLast[i - 1, j - 1] != tabActual[i, j] && selectedNucleons.Contains(tabActual[i, j]))
                {
                    tabBorders[i, j] = -1;
                }
            }
            //check left down
            if (i != 0 && j != sizeY - 1)
            {
                if (tabLast[i - 1, j + 1] != tabActual[i, j] && selectedNucleons.Contains(tabActual[i, j]))
                {
                    tabBorders[i, j] = -1;
                }
            }
            //check right
            if (i != sizeX - 1)
            {
                if (tabLast[i + 1, j] != tabActual[i, j] && selectedNucleons.Contains(tabActual[i, j]))
                {
                    tabBorders[i, j] = -1;
                }
            }
            //check right up
            if (i != sizeX - 1 && j != 0)
            {
                if (tabLast[i + 1, j - 1] != tabActual[i, j] && selectedNucleons.Contains(tabActual[i, j]))
                {
                    tabBorders[i, j] = -1;
                }
            }
            //check right down
            if (i != sizeX - 1 && j != sizeY - 1)
            {
                if (tabLast[i + 1, j + 1] != tabActual[i, j] && selectedNucleons.Contains(tabActual[i, j]))
                {
                    tabBorders[i, j] = -1;
                }
            }
            //check up
            if (j != 0)
            {
                if (tabLast[i, j - 1] != tabActual[i, j] && selectedNucleons.Contains(tabActual[i, j]))
                {
                    tabBorders[i, j] = -1;
                }
            }
            //check down
            if (j != sizeY - 1)
            {
                if (tabLast[i, j + 1] != tabActual[i, j] && selectedNucleons.Contains(tabActual[i, j]))
                {
                    tabBorders[i, j] = -1;
                }
            }
        }





        private void ImportText()
        {
            string testText;
            System.IO.File.ReadAllText("../output.txt");
        }


        private void ExportText()
        {
            string text = null;

            for(int i = 0; i< int.Parse(textBoxSizeX.Text); i++)
            {
                for(int j = 0; j< int.Parse(textBoxSizeY.Text); j++)
                {
                    text = text + globalTab[i, j].ToString();                    
                }

                text = text + Environment.NewLine;                
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text|*txt;";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.AppendAllText(saveFileDialog.FileName, text);
            }
                
        }


        private void ImportBMP()
        {
            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Images|*.png;*.bmp;*.jpg";
            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = Image.FromFile(openFileDialog.FileName);
            }
        }

        private void ExportBMP()
        {            
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Images|*.png;*.bmp;*.jpg";
            if(saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image.Save(saveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
            }
        }

        private void TestDraw()
        {
            
        }

        private void exportTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportText();
        }

        private void importTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportText();
        }

        private void importBMPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportBMP();
        }

        private void exportBMPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportBMP();
        }

        private void secondSimulationButton_Click(object sender, EventArgs e)
        {
            DualPhase();
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            nucleonAmountGlobal = 0;
            exceptions.Clear();
            nucleonsLastColors.Clear();
            simulationButton.Enabled = true;
            secondSimulationButton.Enabled = false;
            bordersButton.Enabled = false;
        }

        private void bordersButton_Click(object sender, EventArgs e)
        {
            SelectBorders();
        }
    }
}
