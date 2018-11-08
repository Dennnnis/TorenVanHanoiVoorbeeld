using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TorenVanHanoi_Simpel
{
    public partial class Form : System.Windows.Forms.Form
    {
        Color diskKleur = Color.Peru;
        const int clipAfstandVanToren = 100; //De afstand (in X) die nodig is om op de toren te plaatsen
        const int afstandVanVloer = 62;    //De afstand (in Y) hoe hoog de disks boven de grond zijn
        const int afstandTussenDisks = 24;
        readonly int[] torenLocaties;

        readonly Stack<Button>[] stacks = new Stack<Button>[]
        {
            new Stack<Button>(),
            new Stack<Button>(),
            new Stack<Button>()
        };

        Button geselecteerdeDisk = null;   // null is geen disk 

        public Form()
        {
            InitializeComponent();
      
            Click += MuisLosGelaten; //Voeg de click event toe and de form 

            torenLocaties = new int[] //Stel de toren locaties in (Ik gebruik de labels hiervoor)
            {
                label1.Location.X, //Toren 1
                label2.Location.X, //Toren 2
                label3.Location.X, //Toren 3
            };

            foreach (Control c in Controls) //Zoek alle disks(Buttons) en stop ze in de stack
            {
                if (c is Button) //Als het een knop is voeg het toe aan de stack
                {
                    c.Click += EenDiskIsInGedrukt; //Voeg alvast de click functie toe aan de eventHandler
                    stacks[0].Push((Button)c);    //Voeg deze knop toe aan de stack
                }
            }

            ///Buttons zitten nu in de stack maar niet nog niet gesorteerd
            stacks[0] = new Stack<Button>(stacks[0].OrderByDescending(a => Convert.ToInt32(a.Text))); //Sort de eerste (Toren 1) stack
            ClipDeDisks(); //Clip de disks zodat het er niet raar uit ziet bij het begin...
        }

        private void MuisLosGelaten(object sender, EventArgs e)
        {
            if (geselecteerdeDisk != null) //Check of er een disk geselecteerd is
            {
                Point mouseLocation = PointToClient(Cursor.Position); //Krijg de muis locatie

                for (int i = 0; i < torenLocaties.Length; ++i) //Loop over alle toren locaties
                {
                    //Check bij iedere toren of de muis dichtbij genoeg is
                    if (Math.Abs(torenLocaties[i] - mouseLocation.X) < clipAfstandVanToren)
                    {
                        var torenVan = stacks[ZoekToren(Convert.ToInt32(geselecteerdeDisk.Text))];
                        var torenNaar = stacks[i];

                        //(Je kan deze 2 if statements samen voegen maar ik heb ze uit elkaar gehaald voor overzichtelijkheid)  
                        if (torenNaar.Count > 0) //Check of dit wel mag volgends het spel 
                        {
                            if (Convert.ToInt32(geselecteerdeDisk.Text) > Convert.ToInt32(torenNaar.Peek().Text))
                            {
                                MessageBox.Show("Deze move is niet mogelijk!");
                                break;
                            }
                        }

                        //Move de disk
                        torenNaar.Push(torenVan.Pop());
                        ClipDeDisks();
                        break;
                    }
                }

                //Maak de selectie vrij
                geselecteerdeDisk.BackColor = diskKleur;
                geselecteerdeDisk = null;
            }
        }

        private int ZoekToren(int disk) //Vind de toren van een disk via het nummer van de disk
        {
            for (int i = 0; i < stacks.Length; ++i)
            {
                foreach (Button b in stacks[i].ToArray())
                {
                    if (Convert.ToInt32(b.Text) == disk) return i;
                }
            }

            //Als er geen toren is gevonden
            throw new Exception("Missing disk!");
        }

        private void EenDiskIsInGedrukt(object sender, EventArgs e)
        {
            if (geselecteerdeDisk == null) //Je kan alleen een disk selecteren als je nog geen disk geselecteerd hebt
            {
                geselecteerdeDisk = (Button)sender;
                int nummer = Convert.ToInt32(geselecteerdeDisk.Text);

                
                if (stacks[ZoekToren(nummer)].Min(a => Convert.ToInt32(a.Text)) != nummer) //Check of dit niet de kleinste disk is op de stapel
                {
                    geselecteerdeDisk = null; //Deselecteer de disk
                    return; //Exit
                }

                geselecteerdeDisk.BackColor = Color.Green; //Maak de disk groen als hij geselecteerd is
            }
        }

        private void ClipDeDisks() //Deze functie zorgt er voor dat alle disks op de goeie plek komen te staan
        { 
            for (int i = 0; i < stacks.Length; ++i)
            {
                Button[] buttons = stacks[i].ToArray();

                for (int b = buttons.Length-1; b >= 0; --b) //Reverse loop omdat we van onder beginnen
                {
                    int count = (buttons.Length - 1) - b;                    //Hoeveelste disk
                    int x = torenLocaties[i] - (buttons[b].Width/2);        //Trek de helft van de button X er af zodat hij in het midden zit
                    int y = afstandVanVloer + (afstandTussenDisks * count);

                    //Spiegel de Y locatie
                    y = Size.Height - y;

                    buttons[b].Location = new Point(x, y);

                    //NOTE: als je dit beter wil doen kan je inplaats van iedere button
                    //te verplaatsen, alleen de button die de gebruiker verplaatst te verplaatsen..
                }
            }
        }
    }
}
