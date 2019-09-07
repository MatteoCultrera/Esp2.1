using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EspInterface.Models;
using EspInterface.ViewModels;

namespace EspInterface.Views
{
    /// <summary>
    /// Logica di interazione per Setup.xaml
    /// </summary>
    /// 


    public partial class Setup : UserControl
    {
        public Board draggingBoard;
        private BoardInGrid draggingBoardPositioned;
        private bool returningDrag;
        private Point initialPosition;
        private Image draggingImage;
        private List<BoardInGrid> boardsInGrid;
        private static double initialPosX = 55.2, initialPosY = 109.3;
        private static double offset = 29.4;
        private int boardPosX, boardPosY;
        private int[,] gridPos;
        private bool isPositioned;

        public Setup()
        {
            InitializeComponent();
            returningDrag = false;
            draggingImage = null;
            boardsInGrid = new List<BoardInGrid>();
            gridPos = new int[10, 10];
            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                    gridPos[i,j] = 0;
        }

        public void AddBoardInGrid(Board realB, int x, int y) {
            if (gridPos[x, y] == 1)
                return;

            if (x > 9 || y > 9 || x < 0 || y < 0)
                return;
            BoardInGrid board = new BoardInGrid(canvas, realB, x, y, realB.BoardImgSrc, this);
            canvas.Children.Add(board.getCan());
            Canvas.SetLeft(board.getCan(), initialPosX + offset * x);
            Canvas.SetBottom(board.getCan(), initialPosY + offset * y);
            boardsInGrid.Add(board);
            gridPos[x, y] = 1;

            SetupModel sm = (SetupModel)(this.DataContext);

            sm.allPositioned();

        }

        public bool isFree(int x, int y) {
            if (gridPos[x, y] == 1)
                return false;
            else
                return true;
        }
        
        private void handleTextNumBoards(object sender, TextCompositionEventArgs e) {
            Regex regex = new Regex("[^1-9]+");

            if (tb.GetLineLength(0) > 0)
            {
                e.Handled = true;
                return;
            }
            e.Handled = regex.IsMatch(e.Text);
        }

        private void deleteMac(object obj, RoutedEventArgs e)
        {
            Button b = obj as Button;
            Board board = b.DataContext as Board;
            board.MAC = "";
            SetupModel sm = (SetupModel)(this.DataContext);

            sm.checkMacs();

        }

        private void handleTextMAC(object sender, TextCompositionEventArgs e)
        {
            TextBox box = sender as TextBox;
            Regex regex = new Regex("^[a-fA-F0-9:]*$");

            string text = box.GetLineText(0);
   
            if (box.GetLineLength(0) > 16)
            {
                e.Handled = true;
                return;
            }

            e.Handled = !regex.IsMatch(e.Text);
        }

        private void macEntered (object sender, KeyEventArgs e){

            TextBox box = sender as TextBox;
            Regex rxMacAddress = new Regex(@"^[0-9a-fA-F]{2}(((:[0-9a-fA-F]{2}){5})|((:[0-9a-fA-F]{2}){5}))$");

            SetupModel sm = (SetupModel)(this.DataContext);
            

            if (e.Key == Key.Return)
            {
                if ((!rxMacAddress.IsMatch(box.Text) || sm.macRepeated(box.Text.ToString())) && box.Text.Length != 0)
                {
                    Storyboard redFlash = box.TryFindResource("redFlash") as Storyboard;
                    box.Text = "";
                    redFlash.Begin(box);
                }
                else {
                    BindingExpression binding = box.GetBindingExpression(TextBox.TextProperty);
                    Keyboard.ClearFocus();
                    binding.UpdateSource();
                    sm.checkMacs();
                }
            }
           
        }

        private void BoxT_LostFocus(object sender, RoutedEventArgs e)
        {

            TextBox box = sender as TextBox;
            Regex rxMacAddress = new Regex(@"^[0-9a-fA-F]{2}(((:[0-9a-fA-F]{2}){5})|((:[0-9a-fA-F]{2}){5}))$");

            if (!rxMacAddress.IsMatch(box.Text) && box.Text.Length != 0)
            {
                Storyboard redFlash = box.TryFindResource("redFlash") as Storyboard;
                box.Text = "";
                redFlash.Begin(box);
            }
            else
            {
                BindingExpression binding = box.GetBindingExpression(TextBox.TextProperty);
                SetupModel sm = (SetupModel)(this.DataContext);
                Keyboard.ClearFocus();
                binding.UpdateSource();
                sm.checkMacs();
            }
        }

        //FOR DEBUG ONLY
        private void debugButtonClick(object sender, RoutedEventArgs e) {
            Button butt = sender as Button;
            SetupModel sm = (SetupModel)(this.DataContext);
            sm.boardConnected(DebugTB.Text);

        }

        //DragnDrop tries

        private Point mousePosition;

        private void ListBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                (sender as ListBox).ReleaseMouseCapture();
        }

        public void startDragging(object sender, MouseEventArgs e) {
           
            SetupModel sm = (SetupModel)(this.DataContext);

            if (!sm.canDrag())
                return;

            Image im = sender as Image;
            draggingBoard = im.DataContext as Board;

            if (draggingBoard.boardNum - 1 != boardsInGrid.Count)
                return;

            if (canvas.CaptureMouse()) {

                isPositioned = false;
                mousePosition = e.GetPosition(canvas);

                Point p1 = canvas.TranslatePoint(new Point(0, 0), Window.GetWindow(canvas));
                Point p = im.TranslatePoint(new Point(0,0) , Window.GetWindow(im));

                BitmapImage bitmap = new BitmapImage(new Uri(draggingBoard.BoardImgSrc, UriKind.RelativeOrAbsolute));

                draggingImage = new Image
                {
                    Source = bitmap,
                    Width = 35,
                    Height = 35,
                    Visibility = Visibility.Visible

                };

                canvas.Children.Add(draggingImage);

                p.X = p.X - p1.X;
                p.Y = p.Y - p1.Y;

               
                //Now P stores the initial position of the image
                initialPosition = p;

                //MessageBox.Show(" " + draggingBoard.BoardName + " x: " + p.X + " y: " + p.Y+ "\ninitialPos x:"+initialPosition.X + " y: "+initialPosition.Y);

                Canvas.SetLeft(draggingImage, initialPosition.X);
                Canvas.SetTop(draggingImage, initialPosition.Y);
                draggingBoard.dragging = true;
                draggingBoard.subtitle = "";
                Panel.SetZIndex(draggingImage, 1);
            }

        }


        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (draggingImage!=null && !returningDrag)
            {
                var position = e.GetPosition(canvas);
                var offset = position - mousePosition;
                mousePosition = position;
                double newPosX, newPosY;

                newPosX = Canvas.GetLeft(draggingImage) + offset.X;
                newPosY = Canvas.GetTop(draggingImage) + offset.Y;

                if (newPosX < 0)
                    newPosX =  0;
                if (newPosX > canvas.Width)
                    newPosX = canvas.Width;
                if (newPosY < 0)
                    newPosY = 0;
                if (newPosY > canvas.Height)
                    newPosY = canvas.Height;

                Canvas.SetLeft(draggingImage, newPosX);
                Canvas.SetTop(draggingImage, newPosY);
            }
        }
        
        private void CanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).Activate();
            Window.GetWindow(this).Focus();
            canvas.Focus();

            if (draggingImage!=null)
            {
                returningDrag = true;
                canvas.ReleaseMouseCapture();
                if (!isPositioned)
                {
                    if (!inGrid(Canvas.GetLeft(draggingImage), Canvas.GetTop(draggingImage)))
                    {
                        DoubleAnimation da1 = new DoubleAnimation()
                        {
                            From = Canvas.GetLeft(draggingImage),
                            To = initialPosition.X,
                            Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.2)),
                        };

                        DoubleAnimation da2 = new DoubleAnimation()
                        {
                            From = Canvas.GetTop(draggingImage),
                            To = initialPosition.Y,
                            Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.2)),
                        };

                        da1.Completed += new EventHandler(endDragging);

                        draggingImage.BeginAnimation(Canvas.LeftProperty, da1);
                        draggingImage.BeginAnimation(Canvas.TopProperty, da2);
                    }
                    else
                    {
                        if (nearestSpotInGrid())
                        {

                            DoubleAnimation da1 = new DoubleAnimation()
                            {
                                From = Canvas.GetLeft(draggingImage),
                                To = initialPosition.X,
                                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.1)),
                            };

                            DoubleAnimation da2 = new DoubleAnimation()
                            {
                                From = Canvas.GetTop(draggingImage),
                                To = initialPosition.Y,
                                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.1)),
                            };

                            da1.Completed += new EventHandler(endDraggingInGrid);
                            draggingImage.BeginAnimation(Canvas.LeftProperty, da1);
                            draggingImage.BeginAnimation(Canvas.TopProperty, da2);
                        }
                        else
                        {

                            DoubleAnimation da1 = new DoubleAnimation()
                            {
                                From = Canvas.GetLeft(draggingImage),
                                To = initialPosition.X,
                                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.2)),
                            };

                            DoubleAnimation da2 = new DoubleAnimation()
                            {
                                From = Canvas.GetTop(draggingImage),
                                To = initialPosition.Y,
                                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.2)),
                            };

                            da1.Completed += new EventHandler(endDragging);

                            draggingImage.BeginAnimation(Canvas.LeftProperty, da1);
                            draggingImage.BeginAnimation(Canvas.TopProperty, da2);
                        }

                    }
                }
                else
                {
                     if (!inGrid(Canvas.GetLeft(draggingImage), Canvas.GetTop(draggingImage)))
                    {
                        DoubleAnimation da1 = new DoubleAnimation()
                        {
                            From = Canvas.GetLeft(draggingImage),
                            To = initialPosition.X,
                            Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.2)),
                        };

                        DoubleAnimation da2 = new DoubleAnimation()
                        {
                            From = Canvas.GetTop(draggingImage),
                            To = initialPosition.Y,
                            Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.2)),
                        };

                        da1.Completed += new EventHandler(endDraggingPositioned);

                        draggingImage.BeginAnimation(Canvas.LeftProperty, da1);
                        draggingImage.BeginAnimation(Canvas.TopProperty, da2);
                    }
                    else
                    {
                        if (nearestSpotInGrid())
                        {

                            DoubleAnimation da1 = new DoubleAnimation()
                            {
                                From = Canvas.GetLeft(draggingImage),
                                To = initialPosition.X,
                                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.1)),
                            };

                            DoubleAnimation da2 = new DoubleAnimation()
                            {
                                From = Canvas.GetTop(draggingImage),
                                To = initialPosition.Y,
                                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.1)),
                            };

                            da1.Completed += new EventHandler(endDraggingInGridPositioned);
                            draggingImage.BeginAnimation(Canvas.LeftProperty, da1);
                            draggingImage.BeginAnimation(Canvas.TopProperty, da2);
                        }
                        else
                        {

                            DoubleAnimation da1 = new DoubleAnimation()
                            {
                                From = Canvas.GetLeft(draggingImage),
                                To = initialPosition.X,
                                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.2)),
                            };

                            DoubleAnimation da2 = new DoubleAnimation()
                            {
                                From = Canvas.GetTop(draggingImage),
                                To = initialPosition.Y,
                                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.2)),
                            };

                            da1.Completed += new EventHandler(endDraggingPositioned);

                            draggingImage.BeginAnimation(Canvas.LeftProperty, da1);
                            draggingImage.BeginAnimation(Canvas.TopProperty, da2);
                        }

                    }

                }

                
            }
        }

        private bool nearestSpotInGrid() {
            double x = Canvas.GetLeft(draggingImage), y = Canvas.GetTop(draggingImage);
            double posX=55.2, posY=109.8;
            double minX = 5000, minY = 5000;


            if (x < 55.2)
                x = 55.2;
            if (y > 575 - 109.8)
                y = 575 - 109.8;

            for (int i = 0; i < 10; i++) {
                if (Math.Abs(x - (55.2 + offset * i)) < minX)
                {
                    boardPosX = i;
                    posX = (55.2 + offset * i);
                    minX = Math.Abs(x - (55.2 + offset * i));
                }
            }
            

            for (int i = 0; i < 10; i++)
            {
                if (Math.Abs(y - (575 - 109.8 - offset * i)) < minY)
                {
                    boardPosY = i;
                    posY = 575 - 109.8 - offset * i;
                    minY = Math.Abs(y - (575 - 109.8 - offset * i));
                }
            }


            if (isFree(boardPosX, boardPosY))
            {
                initialPosition.X = posX;
                initialPosition.Y = posY;
                return true;
            }

            else return false;

            

        }

        private bool inGrid(double x, double y) {
            if (x >= Canvas.GetLeft(grid) && x <= Canvas.GetLeft(grid) + grid.Width)
                if (y >= Canvas.GetTop(grid) && y <= Canvas.GetTop(grid) + grid.Height)
                {
                    boardPosX = 0; boardPosY = 0;
                    return true;
                }
            return false;
        }

        private void endDraggingInGrid(object sender, EventArgs e) {

            returningDrag = false;
            SetupModel sm = (SetupModel)(this.DataContext);
            canvas.Children.Remove(draggingImage);
            if (draggingBoard != null)
            {
                AddBoardInGrid(draggingBoard, boardPosX, boardPosY);
            }
            draggingImage = null;
        }

   
        private void endDragging(Object sender, EventArgs e) {

            returningDrag = false;
            SetupModel sm = (SetupModel)(this.DataContext);
            canvas.Children.Remove(draggingImage);
            if (draggingBoard != null)
            {
                draggingBoard.dragging = false;
                draggingBoard.subtitle = "drag to position";
            }
            draggingImage = null;
            
        }

        private void endDraggingPositioned(Object sender, EventArgs e) {

            returningDrag = false;
            canvas.Children.Remove(draggingImage);
            if (draggingBoardPositioned != null) {

                //Make Board Appear
                draggingBoardPositioned.imageAppear();

            }

            draggingImage = null;
        }

        private void endDraggingInGridPositioned(object sender, EventArgs e) {
            returningDrag = false;
            SetupModel sm = (SetupModel)(this.DataContext);
            canvas.Children.Remove(draggingImage);
            if (draggingBoard != null)
            {
                //Move Board and Appear
                //CHANGE GridPos to update noew positions
                //AddBoardInGrid(draggingBoard, boardPosX, boardPosY)

                Canvas.SetLeft(draggingBoardPositioned.getCan(), initialPosX + offset * boardPosX);
                Canvas.SetBottom(draggingBoardPositioned.getCan(), initialPosY + offset * boardPosY);


                gridPos[draggingBoardPositioned.getX(), draggingBoardPositioned.getY()] = 0;
                gridPos[boardPosX, boardPosY] = 1;

                draggingBoardPositioned.setX(boardPosX);
                draggingBoardPositioned.setY(boardPosY);

                draggingBoardPositioned.imageAppear();


            }
            draggingImage = null;
        }

        

        public class BoardInGrid
        {
            Board posBoard;
            int posX, posY;
            Canvas can;
            Image boardImage;
            BitmapImage bitmap;
            Button boardButton;
            Setup set;

            public BoardInGrid(Canvas father, Board posBoard, int posX, int posY, string imgSource, Setup set) {

                can = new Canvas();

                bitmap = new BitmapImage(new Uri(imgSource, UriKind.RelativeOrAbsolute));

                this.posBoard = posBoard;

                posBoard.positioned = true;
                posBoard.posX = posX;
                posBoard.posY = posY;

                this.set = set;

                boardImage = new Image
                {
                    Source = bitmap,
                    Width = 35,
                    Height = 35,
                    Visibility = Visibility.Visible

                };


                can.Children.Add(boardImage);

                this.posX = posX;
                this.posY = posY;

                boardImage.MouseDown += new MouseButtonEventHandler(startDraggingPositioned);

            }

            public Image getBoardImage() {
                return boardImage;
            }

            public void imageAppear() {
                boardImage.Visibility = Visibility.Visible;
            }

            public Canvas getCan() {
                return can;
            }

            public void setX(int x) {
                this.posX = x;
                posBoard.posX = x;
            }

            public void setY(int y) {
                this.posY = y;
                posBoard.posY = y;
            }

            public int getX() {
                return this.posX;
            }

            public int getY(){
                return this.posY;
            }

            public void startDraggingPositioned(object sender, MouseEventArgs e)
            {

                Image im = sender as Image;
                set.draggingBoardPositioned = this;

                if (set.canvas.CaptureMouse())
                {

                    set.isPositioned = true;
                    set.mousePosition = e.GetPosition(set.canvas);

                    Point p1 = set.canvas.TranslatePoint(new Point(0, 0), Window.GetWindow(set.canvas));
                    Point p = im.TranslatePoint(new Point(0, 0), Window.GetWindow(im));

                    set.draggingImage = new Image
                    {
                        Source = bitmap,
                        Width = 35,
                        Height = 35,
                        Visibility = Visibility.Visible

                    };

                    set.canvas.Children.Add(set.draggingImage);
                    boardImage.Visibility = Visibility.Collapsed;

                    p.X = p.X - p1.X;
                    p.Y = p.Y - p1.Y;


                    //Now P stores the initial position of the image
                    set.initialPosition = p;

                    //MessageBox.Show(" " + draggingBoard.BoardName + " x: " + p.X + " y: " + p.Y+ "\ninitialPos x:"+initialPosition.X + " y: "+initialPosition.Y);

                    Canvas.SetLeft(set.draggingImage, set.initialPosition.X);
                    Canvas.SetTop(set.draggingImage, set.initialPosition.Y);
                    Panel.SetZIndex(set.draggingImage, 1);
                }
            }


        };

    }
}


