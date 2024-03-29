﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;

namespace GraphicsBook
{
    /// <summary>
    /// Display and interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        GraphPaper gp = null;

        Polygon myTriangle = null;
        List<GImage> myImages = new List<GImage>();
        Mesh myMesh = null;
        Quiver myQuiver = null;

        int currentIdx = 0;

        // Are we ready for interactions like slider-changes to alter the 
        // parts of our display (like polygons or images or arrows)? Probably not until those things 
        // have been constructed!
        bool ready = false;

        // Code to create and display objects goes here.
        public Window1()
        {
            InitializeComponent();
            InitializeCommands();

            // Now add some graphical items in the main drawing area, whose name is "Paper"
            gp = this.FindName("Paper") as GraphPaper;


            // Track mouse activity in this window
            MouseLeftButtonDown += MyMouseButtonDown;
            MouseLeftButtonUp += MyMouseButtonUp;
            MouseMove += MyMouseMove;

            #region Triangles, segments, dots
            // A triangle, whose top point can be moved using the slider. 
            myTriangle = new Polygon();
            myTriangle.Points.Add(new Point(0, 10));
            myTriangle.Points.Add(new Point(10, 0));
            myTriangle.Points.Add(new Point(-10, 0));
            myTriangle.Stroke = Brushes.Black;
            myTriangle.StrokeThickness = 1; // 1 mm thick line
            myTriangle.Fill = Brushes.LightSeaGreen;
            gp.Children.Add(myTriangle);

            // A draggable Dot, which is the basepoint of an arrow.
            Dot dd = new Dot(new Point(-40, 60));
            dd.MakeDraggable(gp);
            gp.Children.Add(dd);

            Arrow ee = new Arrow(dd, new Point(10, 10), Arrow.endtype.END);
            gp.Children.Add(ee);

            // a dot and a segment that's attached to it; the dot is animated
            Dot p1 = new Dot(new Point(20, 20));
            gp.Children.Add(p1);
            Point p2 = new Point(50, 50);
            Segment mySegment = new Segment(p1, p2);
            gp.Children.Add(mySegment);

            PointAnimation animaPoint1 = new PointAnimation(
                new Point(-20, -20),
                new Point(-40, 20),
                new Duration(new TimeSpan(0, 0, 5)));
            animaPoint1.AutoReverse = true;
            animaPoint1.RepeatBehavior = RepeatBehavior.Forever;
            p1.BeginAnimation(Dot.PositionProperty, animaPoint1);
            #endregion
            #region Images

            // And a photo from a file, then another that's 
            // created on-the-fly instead of read from a file. 

            GImage myImage = new GImage("./foo.jpg");
            myImage.Width = GraphPaper.wpf(200);
            myImage.Position = new Point(10, 40);
            myImages.Add(myImage);

            myImage = new GImage("./bar.jpg");
            myImage.Width = GraphPaper.wpf(200);
            myImage.Position = new Point(10, 40);
            myImages.Add(myImage);

            Point pq = new Point(10, 40);

            gp.Children.Add(myImages[0]);

            // Now add a second image, based on first building an array of color values
            // Create source array
            byte[,,] stripes = createStripeImageArray();

            myImage = new GImage(stripes);

            // Establish the width and height for this image on the GraphPaper
            myImage.Width = GraphPaper.wpf(128);
            myImage.Height = GraphPaper.wpf(128);
            myImage.Position = new Point(-40, 20);
            myImages.Add(myImage);

            gp.Children.Add(myImages[2]);
            #endregion
            #region Mesh, Quiver, and Text labels

            myMesh = this.createSampleMesh();
            gp.Children.Add(myMesh);

            Text myText = new Text("THIS IS TEXT");
            myText.Position = new Point(20, 50);
            gp.Children.Add(myText);

            myQuiver = makeQuiver();
            foreach (Shape q in myQuiver)
            {
                gp.Children.Add(q);
            }

            #endregion
            ready = true; // Now we're ready to have sliders and buttons influence the display.
        }

        #region Interaction handling -- sliders and buttons

        /* Click-handling in the main graph-paper window */
        public void MyMouseButtonUp(object sender, RoutedEventArgs e)
        {
            if (sender != this) return;
            System.Windows.Input.MouseButtonEventArgs ee =
              (System.Windows.Input.MouseButtonEventArgs)e;
            Debug.Print("MouseUp at " + ee.GetPosition(this));
            e.Handled = true;
        }

        public void MyMouseButtonDown(object sender, RoutedEventArgs e)
        {
            if (sender != this) return;
            System.Windows.Input.MouseButtonEventArgs ee =
              (System.Windows.Input.MouseButtonEventArgs)e;
            Debug.Print("MouseDown at " + ee.GetPosition(this));
            e.Handled = true;
        }


        public void MyMouseMove(object sender, MouseEventArgs e)
        {
            if (sender != this) return;
            System.Windows.Input.MouseEventArgs ee =
              (System.Windows.Input.MouseEventArgs)e;
            // Uncommment following line to get a flood of mouse-moved messages. 
            // Debug.Print("MouseMove at " + ee.GetPosition(this));
            e.Handled = true;
        }

        /* Event handler for a click on button one */
        public void b1Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("Button one clicked!\n");

            gp.Children.Remove(myImages[currentIdx]);

            if ((currentIdx + 1) % myImages.Count == 2)
                currentIdx = (currentIdx + 2) % myImages.Count;
            else
                currentIdx = (currentIdx + 1) % myImages.Count;

            gp.Children.Add(myImages[currentIdx]);

            e.Handled = true; // don't propagate the click any further
        }


        void slider1change(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Debug.Print("Slider changed, ready = " + ready + ", and val = " + e.NewValue + ".\n");
            e.Handled = true;
            // Be sure to not respond to slider-moves until all objects have been constructed. 
            if (ready)
            {
                PointCollection p = myTriangle.Points.Clone();
                Debug.Print(myTriangle.Points.ToString());
                Point u = p[0];
                u.X = e.NewValue;
                p[0] = u;
                myTriangle.Points = p;
            }
        }

        public void b2Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("Button two clicked!\n");
            e.Handled = true; // don't propagate the click any further
        }
        #endregion

        #region Menu, command, and keypress handling

        protected static RoutedCommand ExitCommand;

        protected void InitializeCommands()
        {
            InputGestureCollection inp = new InputGestureCollection();
            inp.Add(new KeyGesture(Key.X, ModifierKeys.Control));
            ExitCommand = new RoutedCommand("Exit", typeof(Window1), inp);
            CommandBindings.Add(new CommandBinding(ExitCommand, CloseApp));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, CloseApp));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, NewCommandHandler));
        }

        void NewCommandHandler(Object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("You selected the New command",
                                Title,
                                MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);

        }

        // Announce keypresses, EXCEPT for CTRL, ALT, SHIFT, CAPS-LOCK, and "SYSTEM" (which is how Windows 
        // seems to refer to the "ALT" keys on my keyboard) modifier keys
        // Note that keypresses that represent commands (like ctrl-N for "new") get trapped and never get
        // to this handler.
        void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if ((e.Key != Key.LeftCtrl) &&
                (e.Key != Key.RightCtrl) &&
                (e.Key != Key.LeftAlt) &&
                (e.Key != Key.RightAlt) &&
                (e.Key != Key.System) &&
                (e.Key != Key.Capital) &&
                (e.Key != Key.LeftShift) &&
                (e.Key != Key.RightShift))
            {
                MessageBox.Show(String.Format("[{0}]  {1} received @ {2}",
                                        e.Key,
                                        e.RoutedEvent.Name,
                                        DateTime.Now.ToLongTimeString()),
                                Title,
                                MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);
            }
        }

        void CloseApp(Object sender, ExecutedRoutedEventArgs args)
        {
            if (MessageBoxResult.Yes ==
                MessageBox.Show("Really Exit?",
                                Title,
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question)
               ) Close();
        }
        #endregion //Menu, command and keypress handling

        #region Image, Mesh, and Quiver, construction helpers
        private byte[,,] createStripeImageArray()
        {
            int width = 128;
            int height = 128;
            byte[,,] pixelArray = new byte[width, height, 4];

            for (int y = 0; y < height; ++y)
            {
                int yIndex = y * width;
                for (int x = 0; x < width; ++x)
                {
                    byte b = (byte)(32 * (Math.Round((x + 2 * y) / 32.0)));
                    pixelArray[x, y, 0] = b;
                    pixelArray[x, y, 1] = b;
                    pixelArray[x, y, 2] = b;
                    pixelArray[x, y, 3] = 255;
                }
            }
            return pixelArray;
        }

        private int vectorIndex(int row, int col, int nrows, int ncols)
        {
            return col + row * ncols;
        }

        private Mesh createSampleMesh()
        {
            int nrows = 4;
            int ncols = 6;
            int nverts = nrows * ncols;
            int nedges = nrows * (ncols - 1) + ncols * (nrows - 1);
            int baseX = -40;
            int baseY = 55;
            Point[] verts = new Point[nverts];
            int[,] edges = new int[nedges, 2];

            for (int y = 0; y < nrows; y++)
            {
                for (int x = 0; x < ncols; x++)
                {
                    verts[vectorIndex(y, x, nrows, ncols)] =
                        new Point(baseX + 10 * x, baseY + 10 * y + 5 * Math.Sin(2 * Math.PI * x / (ncols - 1)));
                }
            }

            int count = 0;
            for (int y = 0; y < nrows; y++)
            {
                for (int x = 0; x < ncols - 1; x++)
                {
                    edges[count, 0] = vectorIndex(y, x, nrows, ncols);
                    edges[count, 1] = vectorIndex(y, x + 1, nrows, ncols);
                    count++;
                }
            }
            for (int x = 0; x < ncols; x++)
            {
                for (int y = 0; y < nrows - 1; y++)
                {
                    edges[count, 0] = vectorIndex(y, x, nrows, ncols);
                    edges[count, 1] = vectorIndex(y + 1, x, nrows, ncols);
                    count++;
                }
            }
            Debug.Print("count = " + count + "\n");
            return new Mesh(nverts, count, verts, edges);
        }

        private Quiver makeQuiver()
        {
            int count = 10;
            Point[] verts = new Point[count];
            Vector[] arrows = new Vector[count];
            for (int i = 0; i < count; i++)
            {
                double th = 2 * Math.PI * i / count;
                verts[i] = new Point(-40 + 5 * Math.Cos(th), -40 + 5 * Math.Sin(th));
                arrows[i] = new Vector(20 * Math.Cos(th), 20 * Math.Sin(th));
            }
            return new Quiver(verts, arrows);
        }
        #endregion
    }
}