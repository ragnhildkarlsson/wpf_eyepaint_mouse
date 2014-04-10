using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace wpf_eyepaint_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Point gaze;
        bool paint = false;
        bool menuActive;
        bool isKeyDown = false; //TODO see if other soultion is possible?
        
        //Painting
        static readonly int pictureWidth = (int)System.Windows.SystemParameters.PrimaryScreenWidth;
        static readonly int pictureHeight = (int)(System.Windows.SystemParameters.PrimaryScreenHeight * 0.8); //TODO CHANGE 0.8 TO Constant
        RenderTargetBitmap painting = new RenderTargetBitmap(pictureWidth, pictureHeight, 96, 96, PixelFormats.Pbgra32);
        

        //Tools 
        List<PaintTool> paintTools;
        List<ColorTool> colorTools;
        
        //Buttons
        Button activeButton;        
        List<Button> toolButtons;
        List<Button> colorButtons;


        Model model;
        View view;

        //Timers
        private DispatcherTimer paintTimer;
        private DispatcherTimer inactivityTimer;
        
        public MainWindow()
        {
            //Initialize model and view
            SettingsFactory sf = new SettingsFactory();
            paintTools = sf.getPaintTools();
            colorTools = sf.getColorTools();
            model = new Model(paintTools[0], colorTools[0]);
            view = new View(painting);

            toolButtons = new List<Button>();
            colorButtons = new List<Button>();
                        
            //Initialize GUI
            InitializeComponent();
            initializeMenu();
            paintingImage.Source = painting;
         
            

            //Initalize eventhandlers
            MouseMove += (object s, MouseEventArgs e) => trackGaze(new Point(Mouse.GetPosition(paintingImage).X,Mouse.GetPosition(paintingImage).Y), paint, 0);
            this.KeyDown += MainWindow_KeyDown;
            this.KeyUp += MainWindow_KeyUp;
        
            //Initialize parameters
            menuActive = false;

             //MouseDown += (object s, MouseEventArgs e) => startPainting();
            //MouseUp += (object s, MouseEventArgs e) => stopPainting();
     
            //Initialize timers
            paintTimer = new DispatcherTimer();
            paintTimer.Interval = TimeSpan.FromMilliseconds(1);
            paintTimer.Tick += (object s, EventArgs e) => { model.Grow(); rasterizeModel(); Console.WriteLine("a tick was ticked"); };

            // Set timer for inactivity
            inactivityTimer = new DispatcherTimer();
            inactivityTimer.Interval = TimeSpan.FromMinutes(15);
            inactivityTimer.Tick += (object s, EventArgs e) =>
            {
              //TODO implement
            };

        }

        void initializeMenu()
        {
            int leftmargin = (int)menuPanel.Margin.Left;
            int rightmargin = (int)menuPanel.Margin.Right;
            int btnWidth = (pictureWidth - leftmargin - rightmargin) / (colorTools.Count() + paintTools.Count + paintToolPanel.Children.Count + colorToolPanel.Children.Count);
            
            
            //Add Colortools
            DockPanel.SetDock(colorToolPanel, Dock.Left);
            
            foreach (ColorTool ct in colorTools)
            {
                Button btn = new Button();
                var brush = new ImageBrush();

                String path = Directory.GetCurrentDirectory() + "\\icons\\" + ct.iconImage; //TODO ev change to resources
                brush.ImageSource = new BitmapImage(new Uri(path));
                btn.Background = brush;
                btn.Focusable = false;
                btn.Width = btnWidth;
                btn.Click += (object s, RoutedEventArgs e) =>
                {
                    model.ChangeColorTool(ct);
                };
                colorToolPanel.Children.Add(btn);
                colorButtons.Add(btn);
            }
            foreach(PaintTool pt in paintTools)
            {
                Button btn = new Button();
                var brush = new ImageBrush();

                String path = Directory.GetCurrentDirectory() + "\\icons\\" + pt.iconImage; //TODO ev change to resources
                brush.ImageSource = new BitmapImage(new Uri(path));
               // brush.ImageSource = new BitmapImage());
                
                btn.Background = brush;
                btn.Focusable = false;
                btn.Width = btnWidth;
                btn.Click += (object s, RoutedEventArgs e) =>
                {
                    model.ChangePaintTool(pt);
                };
                paintToolPanel.Children.Add(btn);
                toolButtons.Add(btn);
            }
            saveButton.Width = btnWidth;
            setRandomBackgroundButton.Width = btnWidth;

        }
        //ButtonMethods on click
        void onSetRandomBackGroundClick(object sender, RoutedEventArgs e)
        {
            setBackGroundToRandomColor();
            model.ResetModel();
        }

        void onSaveClick(object sender, RoutedEventArgs e)
        {
            //TODO CHANGE
            Application.Current.Shutdown();
        }
      

        //Methods for keypress
        void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            isKeyDown = false;
            stopPainting();
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (isKeyDown) return;
            isKeyDown = true;
            startPainting();
            
        }
        void startPainting()
        {
            if (menuActive) return; 
            if (paint) return;
            paint = true;
            paintTimer.Start();
            trackGaze(gaze, paint, 0);
            inactivityTimer.Stop();
        }

        // Stop painting.
        void stopPainting()
        {
            paint = false;
            paintTimer.Stop();
            inactivityTimer.Start();
        }
        void trackGaze(Point p, bool keep = true, int keyhole = 100)
        {
            var distance = Math.Sqrt(Math.Pow(gaze.X - p.X, 2) + Math.Pow(gaze.Y - p.Y, 2));
            if (distance < keyhole) return;
            gaze = p;
            if (keep) model.Add(gaze, true); //TODO Add alwaysAdd argument, or remove it completely from the function declaration.      
        }

        void rasterizeModel()
        {
            Console.WriteLine("raseterize was called");
            view.Rasterize(model.GetRenderQueue());
        }

        void setBackGroundToRandomColor()
        {
            view.setBackGorundColorRandomly();
        }

        void savePainting()
        {
            //TODO implement
        }


    }
}
