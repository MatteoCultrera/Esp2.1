using System;
using System.IO;
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

using System.Diagnostics;

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
        private static double initialPosX = 55.2, initialPosY = 110.3;
        private static double offset = 26.45;
        private int boardPosX, boardPosY;
        private int[,] gridPos;
        private bool isPositioned;
        private List<LoadedBoard> loadedBoards;
        private double scaleFactorMeters;


        public Setup()
        {
            InitializeComponent();
            returningDrag = false;
            draggingImage = null;
            boardsInGrid = new List<BoardInGrid>();
            gridPos = new int[11, 11];
            for (int i = 0; i <= 10; i++)
                for (int j = 0; j <= 10; j++)
                    gridPos[i, j] = 0;

            loadedBoards = new List<LoadedBoard>();

            //We set the scale factor to the default value 1
            scaleFactorMeters = 1 / 264.5;

            string boardsFileLocation = "../../Data/SavedBoards/boards.txt";

            if (File.Exists(boardsFileLocation)) {
                string[] lines = File.ReadAllLines(boardsFileLocation);

                string toPrint = "";

                string[] separator = { "/" };

                foreach (string line in lines)
                {
                    string[] NameMAC = line.Split(separator, 2, StringSplitOptions.RemoveEmptyEntries);
                    loadedBoards.Add(new LoadedBoard(NameMAC[0], NameMAC[1]));
                }

            }
            else
            {
                Trace.WriteLine("File NOt Existing");
            }

        }

        //Function that subscribes the view to any event from the ViewModel
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetupModel sm = (SetupModel)(this.DataContext);

            sm.ErrorConnection += ViewModel_ErrorConnectionDialog;
            sm.screen2 += enlargeList;
            sm.screen3 += shrinkList;
            sm.setupFinished += checkModified;

            sliderRoom.Minimum = 1;
            sliderRoom.Maximum = 20;

        }

        //Function that unsubscribes the view from any event form the ViewModel
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            SetupModel sm = (SetupModel)(this.DataContext);

            sm.ErrorConnection -= ViewModel_ErrorConnectionDialog;
            sm.screen2 -= enlargeList;
            sm.screen3 -= shrinkList;
            sm.setupFinished -= checkModified;
        }

        private void checkModified(object sender, EspInterface.ViewModels.ErrorEventArgs args)
        {
            List<NewModifiedBoards> myBoards = new List<NewModifiedBoards>();

            foreach (BoardInGrid b in boardsInGrid) {
                if (b.posBoard.indexLoaded == -1)
                {
                    myBoards.Add(new NewModifiedBoards(b.posBoard));
                }
                else if (!loadedBoards[b.posBoard.indexLoaded].Name.Equals(b.posBoard.BoardName) || !loadedBoards[b.posBoard.indexLoaded].MAC.Equals(b.posBoard.MAC))
                {
                    myBoards.Add(new NewModifiedBoards(b.posBoard, loadedBoards[b.posBoard.indexLoaded].MAC, loadedBoards[b.posBoard.indexLoaded].Name));
                }
            }


            if (myBoards.Count == 0)
            {
                args.Confirmed = false;
            }
            else
            {

                DialogRecapModifications recap = new DialogRecapModifications(myBoards);
                bool? result = recap.ShowDialog();
                if (result.HasValue)
                {
                    if(result.Value == true)
                    {
                        foreach(NewModifiedBoards b in myBoards)
                        {
                            if (b.isNew)
                            {
                                loadedBoards.Add(new LoadedBoard(b.oldName, b.oldMAC));
                            }
                            else
                            {
                                loadedBoards[b.board.indexLoaded].Name = b.newName;
                                loadedBoards[b.board.indexLoaded].MAC = b.newMAC;
                            }
                        }

                        string toSave = "";
                        foreach(LoadedBoard b in loadedBoards)
                        {
                            toSave += b.Name + "/" + b.MAC + Environment.NewLine;
                        }

                        string boardsFileLocation = "../../Data/SavedBoards/boards.txt";

                        File.WriteAllText(boardsFileLocation, toSave);

                        MessageBox.Show(toSave);
                    }
                    else
                    {
                        args.Confirmed = false;
                    }
                }
                else
                {
                    args.Confirmed = false;
                }
            }



        }

        private void shrinkList(object sender, EventArgs e)
        {
            BoardList.Width = 195;
        }

        private void enlargeList(object sender, EventArgs e)
        {
            BoardList.Width = 225;
        }

        private void ViewModel_ErrorConnectionDialog(object sender, EspInterface.ViewModels.ErrorEventArgs args)
        {
            DialogErrorConnecting error = new DialogErrorConnecting(args.Message);
            bool? result = error.ShowDialog();
            args.Confirmed = result.HasValue ? result.Value : true;
        }

        private void showPickBoardDialog(object sender, RoutedEventArgs e) {

            SetupModel sm = (SetupModel)(this.DataContext);
            DialogChooseBoard choose = new DialogChooseBoard(loadedBoards);
            bool? result = choose.ShowDialog();
            Board senderBoard = (sender as Button).DataContext as Board;
            if (result == true)
            {
                if (senderBoard.indexLoaded != -1)
                {
                    loadedBoards[senderBoard.indexLoaded].selected = false;
                }

                senderBoard.indexLoaded = loadedBoards.IndexOf(choose.picked);
                loadedBoards[senderBoard.indexLoaded].selected = true;

                senderBoard.BoardName = choose.picked.Name;
                senderBoard.MAC = choose.picked.MAC;

                sm.checkMacs();
            }

        }

        private void slider_updateFactor(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            // Xdist_pix : 264.5 = XdistMeters : sliderValue
            //we need to find XdistMeters, which is
            // XdistMeters = Xdist_pix * sliderValue/264.5 so the factor will be sliderValue/264.5
            //Both for x and y

            scaleFactorMeters = sliderRoom.Value / 264.5;

            tbTopLabel.Text = sliderRoom.Value + " m";

            foreach(BoardInGrid b in boardsInGrid)
            {
                setMetersStart(b);
            }

        }

        private bool roomValid()
        {

            BoardInGrid[] array = boardsInGrid.ToArray();

            for(int i = 0; i < array.Length-1; i++)
            {
                for(int j = i+1; j < array.Length; j++)
                {
                    if (linesIntersect(array[i].roomLine, array[j].roomLine))
                        return false;
                }
            }

            return true;
        }

        private bool linesIntersect(Line l1, Line l2)
        {
            My2dVector p = new My2dVector(l1.X1, l1.Y1);
            My2dVector p2 = new My2dVector(l1.X2, l1.Y2);

            My2dVector q = new My2dVector(l2.X1, l2.Y1);
            My2dVector q2 = new My2dVector(l2.X2, l2.Y2);

            var r = p2 - p;
            var s = q2 - q;
            var rxs = r.Cross(s);
            var qpxr = (q - p).Cross(r);

            // If r x s = 0 and (q - p) x r = 0, then the two lines are collinear.
            /*
            if (rxs.IsZero() && qpxr.IsZero())
            {
                // 1. If either  0 <= (q - p) * r <= r * r or 0 <= (p - q) * s <= * s
                // then the two lines are overlapping,
                if ((0 <= (q - p) * r && (q - p) * r <= r * r) || (0 <= (p - q) * s && (p - q) * s <= s * s))
                    return true;

                // 2. If neither 0 <= (q - p) * r = r * r nor 0 <= (p - q) * s <= s * s
                // then the two lines are collinear but disjoint.
                // No need to implement this expression, as it follows from the expression above.
                return false;
            }*/

            // 3. If r x s = 0 and (q - p) x r != 0, then the two lines are parallel and non-intersecting.
            if (rxs.IsZero() && !qpxr.IsZero())
                return false;

            // t = (q - p) x s / (r x s)
            var t = (q - p).Cross(s) / rxs;

            // u = (q - p) x r / (r x s)

            var u = (q - p).Cross(r) / rxs;

            // 4. If r x s != 0 and 0 <= t <= 1 and 0 <= u <= 1
            // the two line segments meet at the point p + t r = q + u s.
            if (!rxs.IsZero() && (0 <= t && t <= 1) && (0 <= u && u <= 1))
            {
                var intersection = p + t * r;

                if (intersection == p || intersection == p2 || intersection == q || intersection == q2)
                    return false;
                // An intersection was found.
                return true;
            }

            // 5. Otherwise, the two line segments are not parallel but do not intersect.
            return false;
        }

        public void setMeters(BoardInGrid b, Image toTrack)
        {
            //This works only with getTop, so we must subtract the canvas height
            b.posBoard.xMeters = (Canvas.GetLeft(toTrack) - 55.2) * scaleFactorMeters;
            b.posBoard.yMeters = ((575 - Canvas.GetTop(toTrack)) - 110.3) * scaleFactorMeters;

            b.posBoard.subtitle = string.Format("x: {0,7:##0.00} m    y: {1,7:##0.00} m", b.posBoard.xMeters, b.posBoard.yMeters);

            Debug.WriteLine("setMeters: " + b.posBoard.xMeters + " " + b.posBoard.yMeters);
        }

        public void setMetersStart(BoardInGrid b)
        {
            b.posBoard.xMeters = (Canvas.GetLeft(b.getCan()) - 55.2) * scaleFactorMeters;
            b.posBoard.yMeters = (Canvas.GetBottom(b.getCan()) - 110.3) * scaleFactorMeters;

            b.posBoard.subtitle = string.Format("x: {0,7:##0.00} m    y: {1,7:##0.00} m", b.posBoard.xMeters, b.posBoard.yMeters);

            Debug.WriteLine("setMeters: " + b.posBoard.xMeters + " " + b.posBoard.yMeters);
        }

        public void AddBoardInGrid(Board realB, int x, int y) {
            if (gridPos[x, y] == 1)
                return;

            if (x > 10 || y > 10 || x < 0 || y < 0)
                return;
            BoardInGrid board = new BoardInGrid(canvas, realB, x, y, realB.BoardImgSrc, this, boardsInGrid.Count());
            canvas.Children.Add(board.getCan());
            Panel.SetZIndex(board.getCan(), 2);
            Canvas.SetLeft(board.getCan(), initialPosX + offset * x);
            Canvas.SetBottom(board.getCan(), initialPosY + offset * y);

            board.roomLine.X1 = initialPosX + offset * x + Measures.offsetBoard;
            board.roomLine.Y1 = 575 - (initialPosY + offset * y) + Measures.offsetBoard;

            //Todo: add right meters to the board
            setMetersStart(board);
       
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
                bool validRoom = roomValid();
                sm.roomValidity(validRoom);
                colorLines(validRoom);
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

        private void macEntered(object sender, KeyEventArgs e) {

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
            foreach (Board b in sm.boardObjs)
            {
                sm.boardConnected(b.MAC);
            }
            sm.boardConnected(DebugTB.Text);

        }

        private void debugButtonError(object sender, RoutedEventArgs e) {
            SetupModel sm = (SetupModel)(this.DataContext);
            sm.errorBoard(DebugTB.Text);
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
                Point p = im.TranslatePoint(new Point(0, 0), Window.GetWindow(im));

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
            
            if (draggingImage != null && !returningDrag)
            {
                var position = e.GetPosition(canvas);
                var offset = position - mousePosition;
                mousePosition = position;
                double newPosX, newPosY;

                newPosX = Canvas.GetLeft(draggingImage) + offset.X;
                newPosY = Canvas.GetTop(draggingImage) + offset.Y;

                if (newPosX < 0)
                    newPosX = 0;
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

                    setMeters(draggingBoardPositioned, draggingImage);

                }
            }

        }

        private void CanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).Activate();
            Window.GetWindow(this).Focus();
            canvas.Focus();

            if (draggingImage != null)
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
            double posX = 55.2, posY = 109.30;
            double minX = 5000, minY = 5000;


            if (x < 55.2)
                x = 55.2;
            if (y > 575 - 109.8)
                y = 575 - 109.8;

            for (int i = 0; i <= 10; i++) {
                if (Math.Abs(x - (55.2 + offset * i)) < minX)
                {
                    boardPosX = i;
                    posX = (55.2 + offset * i);
                    minX = Math.Abs(x - (55.2 + offset * i));
                }
            }


            for (int i = 0; i <= 10; i++)
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

                setMetersStart(draggingBoardPositioned);

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

                setMetersStart(draggingBoardPositioned);

                //Check if the room is valid only if I have all boards
                if(boardsInGrid.Count == sm.boards)
                {
                    bool validRoom = roomValid();
                    sm.roomValidity(validRoom);
                    colorLines(validRoom);

                }

            }
            draggingImage = null;
            draggingBoardPositioned = null;
        }

        public void colorLines(bool validRoom)
        {
            SolidColorBrush color = new SolidColorBrush();
            if (validRoom)
                color.Color = Colors.White;
            else
                color.Color = Colors.Red;

            foreach (BoardInGrid b in boardsInGrid)
            {
                if(b.roomLine != null)
                {
                    b.roomLine.Stroke = color;
                }
            }

        }
        

        public class BoardInGrid
        {
            public Board posBoard;
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
                    StrokeThickness = 3,
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

            public int getY() {
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

    public class NewModifiedBoards{
        private Board _board;
        private string _oldName;
        private string _oldMAC;
        private bool _isNew;

        public bool isNew
        {
            get { return _isNew; }
            set { this._isNew = value; }
        }

        public string oldBoardSrc
        {
            get
            {
                if (this._isNew)
                {
                    return "/Resources/Icons/newBoard.png";
                }
                else
                {
                    return "/Resources/Icons/boardSideDisabled.png";
                }
            }
        }

        public string newBoardSrc
        {
            get
            {
                if (this._isNew)
                {
                    return "";
                }
                else
                {
                    return "/Resources/Icons/boardSide.png";
                }
            }
        }

        public string oldMAC
        {
            get
            {
                if (!_isNew)
                    return _oldMAC;
                else
                    return board.MAC;
            }
            set { this._oldMAC = value; }
        }

        public string oldName
        {
            get
            {
                if (!_isNew)
                    return this._oldName;
                else
                    return board.BoardName;
            }
            set { this._oldName = value; }
        }

        public string newMAC
        {
            get
            {
                if (!_isNew)
                    return this.board.MAC;
                else
                    return "";
            }
        }

        public string newName
        {
            get
            {
                if (!_isNew)
                    return this.board.BoardName;
                else
                    return "";
            }
            
        }

        public string visibilityNew
        {
            get
            {
                if (_isNew)
                {
                    return "Hidden";
                }
                else
                {
                    return "Visible";
                }
            }
        }

        public Board board
        {
            get { return this._board; }
            set { this._board = value; }
        }

        public NewModifiedBoards(Board board, string oldMac, string oldName)
        {
            this._board = board;
            this._oldMAC = oldMac;
            this._oldName = oldName;
            this._isNew = false;
        }

        public NewModifiedBoards(Board board)
        {
            this._board = board;
            this._oldMAC = "";
            this._oldName = "";
            this._isNew = true;
        }
    }

    public class LoadedBoard
    {
        private string _name, _MAC;
        private bool _selected;

        public string imagePath
        {
            get
            {
                if (!_selected)
                    return "/Resources/Icons/boardSide.png";
                else
                    return "/Resources/Icons/boardSideDisabled.png";
            }
        }

        public string colorFont
        {
            get
            {
                if (!_selected)
                    return "#FFFFFF";
                else
                    return "#9EA3AA";
            }
        }

        public bool unselected
        {
            get { return !selected; }
        }

        public string Name
        {
            get { return _name;}
            set { this._name = value; }
        }

        public string MAC
        {
            get { return _MAC; }
            set { this._MAC = MAC; }
        }

        public bool selected
        {
            get { return this._selected; }
            set { this._selected = value; }
        }

        public LoadedBoard(string Name, string MAC)
        {
            this._name = Name;
            this._MAC = MAC;
        }

    }

    //Helps double check if isZero
    public static class Extensions
    {
        private const double Epsilon = 1e-10;

        public static bool IsZero(this double d)
        {
            return Math.Abs(d) < Epsilon;
        }
    }

    //Useful for calulations with 2D points
    public class My2dVector
    {
        public double X;
        public double Y;

        // Constructors.
        public My2dVector(double x, double y) { X = x; Y = y; }
        public My2dVector() : this(double.NaN, double.NaN) { }

        public static My2dVector operator -(My2dVector v, My2dVector w)
        {
            return new My2dVector(v.X - w.X, v.Y - w.Y);
        }

        public static My2dVector operator +(My2dVector v, My2dVector w)
        {
            return new My2dVector(v.X + w.X, v.Y + w.Y);
        }

        public static double operator *(My2dVector v, My2dVector w)
        {
            return v.X * w.X + v.Y * w.Y;
        }

        public static My2dVector operator *(My2dVector v, double mult)
        {
            return new My2dVector(v.X * mult, v.Y * mult);
        }

        public static My2dVector operator *(double mult, My2dVector v)
        {
            return new My2dVector(v.X * mult, v.Y * mult);
        }

        public static bool operator ==(My2dVector one, My2dVector two)
        {
            if (one.X == two.X && one.Y == two.Y)
                return true;
            return false;
        }

        public static bool operator !=(My2dVector one, My2dVector two)
        {
            if (one.X == two.X && one.Y == two.Y)
                return false;
            return true;
        }

        public double Cross(My2dVector v)
        {
            return X * v.Y - Y * v.X;
        }

        public override bool Equals(object obj)
        {
            var v = (My2dVector)obj;
            return (X - v.X).IsZero() && (Y - v.Y).IsZero();
        }
    }

}


