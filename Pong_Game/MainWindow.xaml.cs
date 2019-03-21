using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
/*
 * Written by Elisha Mayer
 * Example for A game in WPF
 */

namespace Pong_Game
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //c-tor
        public MainWindow()
        {
            InitializeComponent();
        }

        //Initialize game
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            //Get window size
            _gameWidth = (int)CanvasGame.ActualWidth;
            _gameHeight = (int)CanvasGame.ActualHeight;
            this.Size.Content = "" + _gameWidth + "x" + _gameHeight;
            _boardSpeedScreen = _gameHeight / 80;
            //Set Ball Location
            _ballLocX = _gameWidth / 2;
            _ballLocY = _gameHeight / 3;
            SetBallLocation(_ballLocX, _ballLocY);

            DrawBlocks();

            LifeLabel.Content = _life;

            //Start ball move thread
            _ballMoveThread = new Thread(BallMovement);
            _ballMoveThread.Start();

            //Set board in the middle
            _boardLoc = _gameWidth / 2 - 75;
            Canvas.SetLeft(Board, _boardLoc);
        }

        //Draw Blocks
        private void DrawBlocks()
        {
            //Upper line
            for (var i = 1; i * (_blockWh + _blockM) + _blockM < _gameWidth - _blockWh - 2 * _blockM; i++)
            {
                var rec = new Rectangle { Width = _blockWh, Height = _blockWh, Fill = Brushes.Brown };
                CanvasGame.Children.Add(rec);
                Canvas.SetTop(rec, 80);
                Canvas.SetLeft(rec, i * 40 + 10);
            }

            //Second line
            for (var i = 2; i * 40 + 10 < _gameWidth - 3 * 40 + 10; i++)
            {
                var rec = new Rectangle { Width = 30, Height = 30, Fill = Brushes.Brown };
                CanvasGame.Children.Add(rec);
                Canvas.SetTop(rec, 120);
                Canvas.SetLeft(rec, i * 40 + 10);
            }
        }

        //Exit game
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _ballMoveThread.Abort();
            Application.Current.Shutdown();
        }

        #region Variebles

        //Canvas Size
        private int _gameWidth = 1206;
        private int _gameHeight = 768;

        //Pause game
        private bool _pause = true;

        //Speed and direction of ball move
        private double _xMove = 2;
        private double _yMove = 2;

        //Ball variables
        private double _ballLocX = 400;
        private double _ballLocY = 150;
        private int _ballSpeed = 6;

        //Board variables
        private int _boardLoc = 10;
        private double _boardSpeed = 1;
        private int _boardSpeedScreen = 10;

        //Blocks variables
        /// <summary>
        ///     Block Height an Width
        /// </summary>
        private readonly int _blockWh = 30;

        /// <summary>
        ///     Block Margin
        /// </summary>
        private readonly int _blockM = 10;

        //Game life
        private int _life = 10;

        //Ball thread
        private Thread _ballMoveThread;

        #endregion

        #region Ball Move

        //Main Ball Loop
        private void BallMovement()
        {
            while (true)
            {
                if (!_pause)
                {
                    //Check sides
                    CheckLeftBorder();
                    CheckRightBorder();
                    CheckBlocks();
                    CheckBoard();

                    //Move ball
                    _ballLocX += _xMove;
                    _ballLocY += _yMove;

                    //Draw ball in location
                    SetBallLocation(_ballLocX, _ballLocY);
                }

                Thread.Sleep(_ballSpeed);
            }
        }

        #region Check Edges

        //Check the left side
        private void CheckLeftBorder()
        {
            //Revert direction when the ball hits the side
            if (_ballLocX <= 0)
                _xMove *= -1;
        }

        //Check the right side
        private void CheckRightBorder()
        {
            //Revert direction when the ball hits the side
            if (_ballLocX + 30 >= _gameWidth)
                _xMove *= -1;
        }

        //Check if the ball hits the board
        private void CheckBoard()
        {
            //Ball hit the board
            if (_ballLocX + 30 - 5 >= _boardLoc && _ballLocX + 5 <= _boardLoc + 150)
            {
                //ball hits the middle of the board
                if (_ballLocX >= _boardLoc && _ballLocX <= _boardLoc + 120)
                {
                    if (_ballLocY + 60 >= _gameHeight && _yMove > 0)
                    {
                        _yMove = -2;
                        _ballSpeed = 6;
                    }
                }
                //ball hits the side of the board
                else
                {
                    if (_ballLocY + 60 >= _gameHeight && _yMove > 0)
                    {
                        _yMove = -2.5;
                        _ballSpeed = 8;
                    }
                }
            }

            //Ball missed the board
            if (_ballLocY + 30 >= _gameHeight)
                BallOutOfCanvas();
        }

        //Check if the ball hits the blocks
        private void CheckBlocks()
        {
            Action act = () =>
            {
                //go over all the blocks
                foreach (var item in CanvasGame.Children)
                    if (item is Rectangle)
                    {
                        var rec = item as Rectangle;

                        //if it hits the  block
                        if (rec.Visibility == Visibility.Visible && Canvas.GetLeft(rec) <= _ballLocX + 30 &&
                            Canvas.GetLeft(rec) + 30 >= _ballLocX && _ballLocY <= Canvas.GetTop(rec) + 30 &&
                            _ballLocY + 30 >= Canvas.GetTop(rec) && rec.Name != "Board")
                        {
                            rec.Visibility = Visibility.Hidden;
                            //it hits from the top or button
                            if (Math.Abs(Canvas.GetTop(rec) - (_ballLocY - 30)) <= Math.Abs(_yMove) ||
                                Math.Abs(Canvas.GetTop(rec) - (_ballLocY + 30)) <= Math.Abs(_yMove))
                                _yMove *= -1;
                            //it hits from the sides
                            else if (Math.Abs(Canvas.GetLeft(rec) - (_ballLocX + 30)) <= Math.Abs(_xMove) ||
                                     Math.Abs(Canvas.GetLeft(rec) - (_ballLocX - 30)) <= Math.Abs(_xMove))
                                _xMove *= -1;
                            break;
                        }

                        
                    }
                //Check if all are hidden
                var visible = false;
                foreach (var item in CanvasGame.Children)
                    if (item is Rectangle)
                    {
                        var rec = item as Rectangle;
                        if (rec.Visibility == Visibility.Visible && rec.Name != "Board")
                            visible = true;
                    }
            

                //When there are no block in the canvas
                        if (!visible)
                {
                    act = () =>
                    {
                        //set label position
                        Canvas.SetTop(Label, _gameHeight / 2);
                        Canvas.SetLeft(Label, _gameWidth / 2 - 100);

                        //set label content
                        Label.Content = "You Won !!!";
                        Label.Visibility = Visibility.Visible;
                    };
                    Dispatcher.BeginInvoke(act);
                    _ballMoveThread.Abort();
                }
            };
            Dispatcher.Invoke(act);

            //Revert direction when the baal hits the top
            if (_ballLocY <= 0)
                _yMove *= -1;
        }

        #endregion

        //Set the ball location
        private void SetBallLocation(double x, double y)
        {
            void Act()
            {
                Canvas.SetLeft(Ball, x);
                Canvas.SetTop(Ball, y);
            }

            Dispatcher.BeginInvoke((Action)Act);
        }

        //When the ball misses the board
        private void BallOutOfCanvas()
        {
            _life--;

            _pause = true;

            //put the ball beck to default place
            _ballLocX = _gameWidth / 2;
            _ballLocY = _gameHeight / 3;
            SetBallLocation(_ballLocX, _ballLocY);

            Action act = () =>
            {
                //set label position
                Canvas.SetTop(Label, _gameHeight / 2);
                Canvas.SetLeft(Label, _gameWidth / 2 - 100);

                //set label content
                if (_life <= 0)
                    Label.Content = "Game Over !!!";
                else
                    Label.Content = "Life left: " + _life;

                //Show the label
                Label.Visibility = Visibility.Visible;
                PreviewKeyDown -= Window_PreviewKeyDown;
            };
            Dispatcher.BeginInvoke(act);

            //Wait 2 sec
            Thread.Sleep(2000);

            act = () =>
            {
                //update life label
                LifeLabel.Content = _life;

                //Not game over
                if (_life > 0)
                {
                    Label.Visibility = Visibility.Hidden;
                    PreviewKeyDown += Window_PreviewKeyDown;
                }
                //Game Over
                else
                {
                    _life = 10;
                    //Restore Rectangular
                    foreach (var item in CanvasGame.Children)
                        if (item is Rectangle)
                            (item as Rectangle).Visibility = Visibility.Visible;
                    Label.Visibility = Visibility.Hidden;
                    PreviewKeyDown += Window_PreviewKeyDown;
                }

                //Put board in the middle
                _boardLoc = _gameWidth / 2 - 75;
                Canvas.SetLeft(Board, _boardLoc);
            };
            Dispatcher.BeginInvoke(act);
        }

        #endregion

        #region Board Move

        //Button pressed
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _pause = false;
            //Left arrow pressed. Move the board left
            if (e.Key == Key.Left)
            {
                if (Canvas.GetLeft(Board) >= _boardSpeedScreen)
                    Canvas.SetLeft(Board, Canvas.GetLeft(Board) - _boardSpeedScreen * _boardSpeed);
                else
                    Canvas.SetLeft(Board, 0);
            }

            //Right arrow pressed. Move the board right
            if (e.Key == Key.Right)
            {
                if (Canvas.GetLeft(Board) <= _gameWidth - _boardSpeedScreen * _boardSpeed - 150)
                    Canvas.SetLeft(Board, Canvas.GetLeft(Board) + _boardSpeedScreen * _boardSpeed);
                else
                    Canvas.SetLeft(Board, _gameWidth - 150);
            }

            //On continue press
            _boardSpeed += 0.05;

            _boardLoc = (int)Canvas.GetLeft(Board);
        }

        //Button released
        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            _boardSpeed = 1;
        }

        #endregion
    }
}