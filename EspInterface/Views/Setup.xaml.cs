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


    static class Measures {
        public static Double offsetBoard = 17.5;
        public static Double offsetBoardExternal = 17.5;
    }

   

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
            BoardInGrid board = new BoardInGrid(canvas, realB, x, y, realB.BoardImgSrc, this, boardsInGrid.Count());
            canvas.Children.Add(board.getCan());
            Panel.SetZIndex(board.getCan(), 2);
            Canvas.SetLeft(board.getCan(), initialPosX + offset * x);
            Canvas.SetBottom(board.getCan(), initialPosY + offset * y);

            board.roomLine.X1 = initialPosX + offset * x + Measures.offsetBoard;
            board.roomLine.Y1 = 575 - (initialPosY + offset * y) + Measures.offsetBoard;
            boardsInGrid.Add(board);
            gridPos[x, y] = 1;

            SetupModel sm = (SetupModel)(this.DataContext);

            sm.allPositioned();

            if (boardsInGrid.Count != sm.boards) {
                sm.dragNext(boardsInGrid.Count);
            }

            if (board.boardNum != 0) {
                BoardInGrid previous = boardsInGrid[board.boardNum - 1];
                board.setExternalLine(previous.roomLine);
                previous.roomLine.Visibility = Visibility.Visible;
                previous.roomLine.X2 = initialPosX + offset * x + Measures.offsetBoardExternal;
                previous.roomLine.Y2 = 575 - (initialPosY + offset * y) + Measures.offsetBoardExternal;
            }

            if (boardsInGrid.Count() == sm.boards) {
                boardsInGrid[0].setExternalLine(board.roomLine);
                board.roomLine.Visibility = Visibility.Visible;
                board.roomLine.X2 = Canvas.GetLeft(boardsInGrid[0].getCan()) + Measures.offsetBoardExternal;
                board.roomLine.Y2 = 575 - Canvas.GetBottom(boardsInGrid[0].getCan()) + Measures.offsetBoardExternal;
            }
            

            //MessageBox.Show("Added board: " + board.getBoardName() + " at pos " + board.boardNum+ "\n" +s);

        }

        public Point positionedBoard(int boardNum) {
            Point p = new Point()
            {
                Y = Canvas.GetBottom(boardsInGrid[boardNum].getCan()),
                X = Canvas.GetLeft(boardsInGrid[boardNum].getCan())
                
            };


            //MessageBox.Show("Finding posX of board " + boardsInGrid[boardNum].getBoardName() + " with position " + boardsInGrid[boardNum].boardNum + " that is " + Canvas.GetLeft(boardsInGrid[boardNum].getCan()) + " " + Canvas.GetBottom(boardsInGrid[boardNum].getCan()));

            return p;
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

        private void nameEntered(object sender, KeyEventArgs e)
        {

            TextBox box = sender as TextBox;
            SetupModel sm = (SetupModel)(this.DataContext);
            Board b = box.DataContext as Board;

            if (e.Key == Key.Return || e.Key == Key.Tab)
            {
                e.Handled = true;
                if (box.Text.Length == 0)
                {
                    box.Text = b.BoardName;
                    Keyboard.ClearFocus();
                }
                else
                {
                    BindingExpression binding = box.GetBindingExpression(TextBox.TextProperty);
                    Keyboard.ClearFocus();
                    binding.UpdateSource();
                }
            }

        }

        private void BoxName_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            box.CaretIndex = box.Text.Length;
        }

        private void BoxName_LostFocus(object sender, RoutedEventArgs e)
        {

            TextBox box = sender as TextBox;
            SetupModel sm = (SetupModel)(this.DataContext);
            Board b = box.DataContext as Board;

            if (box.Text.Length == 0)
            {
                box.Text = b.BoardName;
                Keyboard.ClearFocus();
            }
            else
            {
                BindingExpression binding = box.GetBindingExpression(TextBox.TextProperty);
                Keyboard.ClearFocus();
                binding.UpdateSource();
            }
            
        }

        private void BoxT_LostFocus(object sender, RoutedEventArgs e)
        {

            TextBox box = sender as TextBox;
            Regex rxMacAddress = new Regex(@"^[0-9a-fA-F]{2}(((:[0-9a-fA-F]{2}){5})|((:[0-9a-fA-F]{2}){5}))$");
            SetupModel sm = (SetupModel)(this.DataContext);

            if ((!rxMacAddress.IsMatch(box.Text) || sm.macRepeated(box.Text.ToString())) && box.Text.Length != 0)
            {
                Storyboard redFlash = box.TryFindResource("redFlash") as Storyboard;
                box.Text = "";
                redFlash.Begin(box);
            }
            else
            {
                BindingExpression binding = box.GetBindingExpression(TextBox.TextProperty);
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

        private void debugButtonError(object sender, RoutedEventArgs e) {
            Button butt = sender as Button;
            DialogErrorConnecting error = new DialogErrorConnecting("Ops, looks like board not connected");
            error.ShowDialog();
            MessageBox.Show("OKOK");
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

                if (draggingBoardPositioned != null) {
                    //MessageBox.Show("newPosX: "+newPosX+" startingX: "+draggingBoardPositioned.startingX);

                    draggingBoardPositioned.roomLine.X1 = newPosX + Measures.offsetBoard;
                    draggingBoardPositioned.roomLine.Y1 = newPosY + Measures.offsetBoard;

                    //Move the end point of the line before it
                    if (draggingBoardPositioned.hasExternalLine()) {
                        draggingBoardPositioned.getExternalLine().X2 = newPosX + Measures.offsetBoardExternal;
                        draggingBoardPositioned.getExternalLine().Y2 = newPosY + Measures.offsetBoardExternal;
                    }

                }
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

                        DoubleAnimation linea1 = new DoubleAnimation()
                        {
                            From = Canvas.GetLeft(draggingImage) + Measures.offsetBoard,
                            To = initialPosition.X + Measures.offsetBoard,
                            Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.2)),
                        };

                        DoubleAnimation linea2 = new DoubleAnimation()
                        {
                            From = Canvas.GetTop(draggingImage) + Measures.offsetBoard,
                            To = initialPosition.Y + Measures.offsetBoard,
                            Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.2)),
                        };


                        DoubleAnimation linea1Ext = new DoubleAnimation()
                        {
                            From = Canvas.GetLeft(draggingImage) + Measures.offsetBoardExternal,
                            To = draggingBoardPositioned.startingX + Measures.offsetBoardExternal,
                            Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.2)),
                        };

                        DoubleAnimation linea2Ext = new DoubleAnimation()
                        {
                            From = Canvas.GetTop(draggingImage) + Measures.offsetBoardExternal,
                            To = draggingBoardPositioned.startingY + Measures.offsetBoardExternal,
                            Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.2)),
                        };

                        da1.Completed += new EventHandler(endDraggingPositioned);

                        draggingImage.BeginAnimation(Canvas.LeftProperty, da1);
                        draggingImage.BeginAnimation(Canvas.TopProperty, da2);
                        draggingBoardPositioned.roomLine.BeginAnimation(Line.X1Property, linea1);
                        draggingBoardPositioned.roomLine.BeginAnimation(Line.Y1Property, linea2);
                        if (draggingBoardPositioned.hasExternalLine())
                        {
                            draggingBoardPositioned.getExternalLine().BeginAnimation(Line.X2Property, linea1Ext);
                            draggingBoardPositioned.getExternalLine().BeginAnimation(Line.Y2Property, linea2Ext);
                        }

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

                            DoubleAnimation linea1 = new DoubleAnimation()
                            {
                                From = Canvas.GetLeft(draggingImage) + Measures.offsetBoard,
                                To = initialPosition.X + Measures.offsetBoard,
                                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.1)),
                            };

                            DoubleAnimation linea2 = new DoubleAnimation()
                            {
                                From = Canvas.GetTop(draggingImage) + Measures.offsetBoard,
                                To = initialPosition.Y + Measures.offsetBoard,
                                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.1)),
                            };

                            DoubleAnimation linea1Ext = new DoubleAnimation()
                            {
                                From = Canvas.GetLeft(draggingImage) + Measures.offsetBoardExternal,
                                To = initialPosition.X + Measures.offsetBoardExternal,
                                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.1)),
                            };

                            DoubleAnimation linea2Ext = new DoubleAnimation()
                            {
                                From = Canvas.GetTop(draggingImage) + Measures.offsetBoardExternal,
                                To = initialPosition.Y + Measures.offsetBoardExternal,
                                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.1)),
                            };

                            da1.Completed += new EventHandler(endDraggingInGridPositioned);
                            draggingImage.BeginAnimation(Canvas.LeftProperty, da1);
                            draggingImage.BeginAnimation(Canvas.TopProperty, da2);
                            draggingBoardPositioned.roomLine.BeginAnimation(Line.X1Property, linea1);
                            draggingBoardPositioned.roomLine.BeginAnimation(Line.Y1Property, linea2);

                            if (draggingBoardPositioned.hasExternalLine())
                            {
                                draggingBoardPositioned.getExternalLine().BeginAnimation(Line.X2Property, linea1Ext);
                                draggingBoardPositioned.getExternalLine().BeginAnimation(Line.Y2Property, linea2Ext);
                                
                            }
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

                            DoubleAnimation linea1 = new DoubleAnimation()
                            {
                                From = Canvas.GetLeft(draggingImage) + Measures.offsetBoard,
                                To = initialPosition.X + Measures.offsetBoard,
                                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.2)),
                            };

                            DoubleAnimation linea2 = new DoubleAnimation()
                            {
                                From = Canvas.GetTop(draggingImage) + Measures.offsetBoard,
                                To = initialPosition.Y + Measures.offsetBoard,
                                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.2)),
                            };

                            DoubleAnimation linea1Ext = new DoubleAnimation()
                            {
                                From = Canvas.GetLeft(draggingImage) + Measures.offsetBoardExternal,
                                To = draggingBoardPositioned.startingX + Measures.offsetBoardExternal,
                                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.2)),
                            };

                            DoubleAnimation linea2Ext = new DoubleAnimation()
                            {
                                From = Canvas.GetTop(draggingImage) + Measures.offsetBoardExternal,
                                To = draggingBoardPositioned.startingY + Measures.offsetBoardExternal,
                                Duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.2)),
                            };

                            da1.Completed += new EventHandler(endDraggingPositioned);

                            draggingImage.BeginAnimation(Canvas.LeftProperty, da1);
                            draggingImage.BeginAnimation(Canvas.TopProperty, da2);
                            draggingBoardPositioned.roomLine.BeginAnimation(Line.X1Property, linea1);
                            draggingBoardPositioned.roomLine.BeginAnimation(Line.Y1Property, linea2);
                            if (draggingBoardPositioned.hasExternalLine()) {
                                
                                draggingBoardPositioned.getExternalLine().BeginAnimation(Line.X2Property, linea1Ext);
                                draggingBoardPositioned.getExternalLine().BeginAnimation(Line.Y2Property, linea2Ext);



                            }
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

                draggingBoardPositioned.roomLine.BeginAnimation(Line.X1Property, null);
                draggingBoardPositioned.roomLine.BeginAnimation(Line.Y1Property, null);

                draggingBoardPositioned.roomLine.X1 = initialPosX + offset * draggingBoardPositioned.getX() + Measures.offsetBoard;
                draggingBoardPositioned.roomLine.Y1 = 575 - (initialPosY + offset * draggingBoardPositioned.getY()) + Measures.offsetBoard;

                if (draggingBoardPositioned.hasExternalLine())
                {
                    draggingBoardPositioned.getExternalLine().BeginAnimation(Line.X2Property, null);
                    draggingBoardPositioned.getExternalLine().BeginAnimation(Line.Y2Property, null);

                    draggingBoardPositioned.getExternalLine().X2 = initialPosX + offset * draggingBoardPositioned.getX() + Measures.offsetBoardExternal;
                    draggingBoardPositioned.getExternalLine().Y2 = 575 - (initialPosY + offset * draggingBoardPositioned.getY()) + Measures.offsetBoardExternal;
                }

            }

            draggingImage = null;
            draggingBoardPositioned = null;
        }

        private void endDraggingInGridPositioned(object sender, EventArgs e) {
            returningDrag = false;
            SetupModel sm = (SetupModel)(this.DataContext);
            canvas.Children.Remove(draggingImage);
            if (draggingBoardPositioned != null)
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

                draggingBoardPositioned.roomLine.BeginAnimation(Line.X1Property, null);
                draggingBoardPositioned.roomLine.BeginAnimation(Line.Y1Property, null);

                draggingBoardPositioned.roomLine.X1 = initialPosX + offset * draggingBoardPositioned.getX() + Measures.offsetBoard;
                draggingBoardPositioned.roomLine.Y1 = 575 - (initialPosY + offset * draggingBoardPositioned.getY()) + Measures.offsetBoard;

                if (draggingBoardPositioned.hasExternalLine())
                {
                    draggingBoardPositioned.getExternalLine().BeginAnimation(Line.X2Property, null);
                    draggingBoardPositioned.getExternalLine().BeginAnimation(Line.Y2Property, null);

                    draggingBoardPositioned.getExternalLine().X2 = initialPosX + offset * draggingBoardPositioned.getX() + Measures.offsetBoardExternal;
                    draggingBoardPositioned.getExternalLine().Y2 = 575 - (initialPosY + offset * draggingBoardPositioned.getY()) + Measures.offsetBoardExternal;

                }
     
            }
            draggingImage = null;
            draggingBoardPositioned = null;
        }

        

        public class BoardInGrid
        {
            Board posBoard;
            int posX, posY;
            public int boardNum;
            Canvas can;
            Image boardImage;
            BitmapImage bitmap;
            Setup set;
            Line externalLine;
            public Line roomLine;
            public double startingX, startingY;

            public BoardInGrid(Canvas father, Board posBoard, int posX, int posY, string imgSource, Setup set, int boardNum) {

                can = new Canvas();

                bitmap = new BitmapImage(new Uri(imgSource, UriKind.RelativeOrAbsolute));

                this.posBoard = posBoard;

                this.boardNum = boardNum;

                externalLine = null;

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

                SolidColorBrush color = new SolidColorBrush();
                color.Color = Colors.White;

                roomLine = new Line()
                {
                    StrokeThickness = 2,
                    Stroke = color
                };

                

                
                Binding sourceX = new Binding("X1");
                sourceX.Source = boardImage;

                /*

                Binding sourceY = new Binding("sourceY");
                sourceY.Source = boardImage;*/

                //roomLine.SetBinding(Line.X1Property, sourceX);
                //roomLine.SetBinding(Line.Y1Property, sourceY);

                

                //roomLine.Visibility = Visibility.Collapsed;

                set.canvas.Children.Add(roomLine);

                boardImage.MouseDown += new MouseButtonEventHandler(startDraggingPositioned);

                roomLine.Visibility = Visibility.Collapsed;

                Panel.SetZIndex(roomLine, 1);

            }

            public bool hasExternalLine() {
                return externalLine != null;
            }

            public void setExternalLine(Line externalLine) {
                this.externalLine = externalLine;
            }

            public Line getExternalLine() {
                return this.externalLine;
            }

            public string getBoardName() {
                return posBoard.BoardName;
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



                    p.X = p.X - p1.X;
                    p.Y = p.Y - p1.Y;


                    //Now P stores the initial position of the image
                    set.initialPosition = p;

                    startingX = set.initialPosition.X;
                    startingY = set.initialPosition.Y;



                    //TODO: change this line because must be visible when second board is placed
                    //roomLine.Visibility = Visibility.Visible;

                    roomLine.X1 = set.initialPosition.X + Measures.offsetBoard;
                    roomLine.Y1 = set.initialPosition.Y + Measures.offsetBoard;

                    if (externalLine != null) {
                        externalLine.X2 = set.initialPosition.X + Measures.offsetBoardExternal;
                        externalLine.Y2 = set.initialPosition.Y + Measures.offsetBoardExternal;
                    }

                    boardImage.Visibility = Visibility.Collapsed;

                    //MessageBox.Show("initial Pos " + set.initialPosition.X + " " + set.initialPosition.Y);

                    Canvas.SetLeft(set.draggingImage, set.initialPosition.X);
                    Canvas.SetTop(set.draggingImage, set.initialPosition.Y);
                    Panel.SetZIndex(set.draggingImage, 2);
                    set.draggingBoardPositioned = this;
                }
            }



        };

        
    }
}


