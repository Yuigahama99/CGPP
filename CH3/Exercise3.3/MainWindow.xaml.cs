using System;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using System.Threading;
using System.Linq;
namespace GraphicsBook
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GraphPaper gp = null; 
        bool ready = false;  // Flag for allowing sliders, etc., to influence display. 

        // Build a table of vertices:
        const int nPoints = 8;
        const int nEdges = 12;
        const int nFaces = 6;
        double[,] vtable = new double[nPoints, 3]
        {
            {-0.7, -0.5, 2.7},
            {-0.7, 0.5, 2.7},
            {0, 0.5, 2},
            {0, -0.5, 2},
            {0, -0.5, 3.4},
            {0, 0.5, 3.4},
            {0.7, 0.5, 2.7},
            {0.7, -0.5, 2.7}
        };
        double[,] vtable_empty = new double[nPoints, 3]
        {
            {0, 0, 0},
            {0, 0, 0},
            {0, 0, 0},
            {0, 0, 0},
            {0, 0, 0},
            {0, 0, 0},
            {0, 0, 0},
            {0, 0, 0}
        };
        // Build a table of edges
        int[,] etable = new int[nEdges, 2]{
                {0, 1}, {1, 2}, {2, 3}, {3, 0}, // one face
                {0, 4}, {1, 5}, {2, 6}, {3, 7},  // joining edges
                {4, 5}, {5, 6}, {6, 7}, {7, 4}
        }; // opposite face
        // Build a table of faces {4 vertices and 1 normal vector}
        // Each face the vertex order is important, for get correct normal vector direction
        int[,] ftable = new int[nFaces, 4]{
                {0, 1, 2, 3},
                {4, 5, 1, 0},
                {7, 6, 5, 4},
                {3, 2, 6, 7},
                {1, 5, 6, 2},
                {4, 0, 3, 7}
        };

        double xmin = -0.5;
        double xmax = 0.5;
        double ymin = -0.5;
        double ymax = 0.5;

        Vector3D E = new Vector3D(0, 0, 0);
        Vector3D axis = new Vector3D(0, -0.7, 0);

        public MainWindow()
        {
            InitializeComponent();
            InitializeCommands();
            // Now add some graphical items in the main Canvas, whose name is "GraphPaper"
            gp = this.FindName("Paper") as GraphPaper;
            
            renderCube(gp, vtable);
        }

        public void renderCube(GraphPaper gp, double[,] vertices)
        {
            Point[] pictureVertices = new Point[nPoints];
            double scale = 100;
            for (int i = 0; i < nFaces; i++)
            {
                Vector3D vertex0 = new Vector3D();
                vertex0.X = vtable[ftable[i, 0], 0];
                vertex0.Y = vtable[ftable[i, 0], 1];
                vertex0.Z = vtable[ftable[i, 0], 2];

                Vector3D vertex1 = new Vector3D();
                vertex1.X = vtable[ftable[i, 1], 0];
                vertex1.Y = vtable[ftable[i, 1], 1];
                vertex1.Z = vtable[ftable[i, 1], 2];

                Vector3D vertex2 = new Vector3D();
                vertex2.X = vtable[ftable[i, 2], 0];
                vertex2.Y = vtable[ftable[i, 2], 1];
                vertex2.Z = vtable[ftable[i, 2], 2];

                Vector3D vertex3 = new Vector3D();
                vertex3.X = vtable[ftable[i, 3], 0];
                vertex3.Y = vtable[ftable[i, 3], 1];
                vertex3.Z = vtable[ftable[i, 3], 2];

                // For current order, the cross product will point inward
                // To create outward normal vector, revert the direction
                Vector3D w = -Vector3D.CrossProduct((vertex2 - vertex1), (vertex1 - vertex0));
                Vector3D v = vertex0 - E;

                double visible = Vector3D.DotProduct(v, w);

                if (visible <= 0)
                {
                    double x0prime = vertex0.X / vertex0.Z;
                    double y0prime = vertex0.Y / vertex0.Z;
                    pictureVertices[ftable[i, 0]].X = scale * (1 - (x0prime - xmin) / (xmax - xmin));
                    pictureVertices[ftable[i, 0]].Y = scale * (y0prime - ymin) / (ymax - ymin); // x / z

                    double x1prime = vertex1.X / vertex1.Z;
                    double y1prime = vertex1.Y / vertex1.Z;
                    pictureVertices[ftable[i, 1]].X = scale * (1 - (x1prime - xmin) / (xmax - xmin));
                    pictureVertices[ftable[i, 1]].Y = scale * (y1prime - ymin) / (ymax - ymin); // x / z

                    double x2prime = vertex2.X / vertex2.Z;
                    double y2prime = vertex2.Y / vertex2.Z;
                    pictureVertices[ftable[i, 2]].X = scale * (1 - (x2prime - xmin) / (xmax - xmin));
                    pictureVertices[ftable[i, 2]].Y = scale * (y2prime - ymin) / (ymax - ymin); // x / z

                    double x3prime = vertex3.X / vertex3.Z;
                    double y3prime = vertex3.Y / vertex3.Z;
                    pictureVertices[ftable[i, 3]].X = scale * (1 - (x3prime - xmin) / (xmax - xmin));
                    pictureVertices[ftable[i, 3]].Y = scale * (y3prime - ymin) / (ymax - ymin); // x / z

                    gp.Children.Add(new Segment(pictureVertices[ftable[i, 0]], pictureVertices[ftable[i, 1]]));
                    gp.Children.Add(new Segment(pictureVertices[ftable[i, 1]], pictureVertices[ftable[i, 2]]));
                    gp.Children.Add(new Segment(pictureVertices[ftable[i, 2]], pictureVertices[ftable[i, 3]]));
                    gp.Children.Add(new Segment(pictureVertices[ftable[i, 3]], pictureVertices[ftable[i, 0]]));
                }
            }
        }

#region Interaction handling -- sliders and buttons
        /* Vestigial handling-code from Testbed2DApp -- unused in this project. */

        /* Event handler for a click on button one */
        public void b1Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("Rotate clicked!\n");

            var segments = gp.Children.OfType<Segment>().ToList();
            foreach (var segment in segments)
            {
                gp.Children.Remove(segment);
            }

            // Calculate Rotation
            double radian = 0.05;

            for (int i = 0; i < nPoints; i++)
            {
                Vector3D temp_vertex = new Vector3D();
                temp_vertex.X = vtable[i, 0];
                temp_vertex.Y = vtable[i, 1];
                temp_vertex.Z = vtable[i, 2];

                Vector3D result = Rotate(temp_vertex, axis, radian);
                vtable[i, 0] = result.X;
                vtable[i, 1] = result.Y;
                vtable[i, 2] = result.Z;
            }

            renderCube(gp, vtable);

            e.Handled = true; // don't propagate the click any further
        }

        public Vector3D Rotate(Vector3D v, Vector3D axis, double radian)
        {
            Vector3D vxp = Vector3D.CrossProduct(axis, v);
            Vector3D vxpxp = Vector3D.CrossProduct(axis, vxp);

            return v + Math.Sin(radian) * vxp + (1 - Math.Cos(radian)) * vxpxp;
        }

        void slider1change(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Debug.Print("Slider changed, ready = " + ready + ", and val = " + e.NewValue + ".\n");
            e.Handled = true;
            if (ready)
            {
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
            ExitCommand = new RoutedCommand("Exit", typeof(MainWindow), inp);
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
    }
}