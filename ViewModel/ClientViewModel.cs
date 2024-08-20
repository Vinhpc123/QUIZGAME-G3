﻿
using QuizGame.Common;
using QuizGame.Model;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace QuizGame.ViewModel
{
    public class ClientViewModel : BindableBase
    {
        private IClientCommunicator ClientCommunicator { get; set; }
        private bool IsQuestionAnswered { get; set; }

        public ClientViewModel(IClientCommunicator clientCommunicator)
        {
            if (clientCommunicator == null) throw new ArgumentNullException("clientCommunicator");
            this.ClientCommunicator = clientCommunicator;

            Func<DispatchedHandler, Task> callOnUiThread = async (handler) => await
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, handler);
            
            this.ClientCommunicator.GameAvailable += async (s, e) => 
                await callOnUiThread(() => this.CanJoin = true); 
            this.ClientCommunicator.NewQuestionAvailable += async (s, e) => 
                await callOnUiThread(() => this.CurrentQuestion = e.Question);
            this.ClientCommunicator.PlayerJoined += async (s, e) =>
                await callOnUiThread(() => 
                {
                    this.IsJoined = e.IsJoined;
                    this.ErrorMessageVisibility = e.IsJoined ? Visibility.Collapsed : Visibility.Visible;
                });
            this.ClientCommunicator.InitializeAsync();
        }

        public string StateName
        {
            get { return this.stateName; }
            set { this.SetProperty(ref this.stateName, value); }
        }
        private string stateName;

        public string PlayerName
        {
            get { return this.playerName ?? string.Empty; }
            set
            {
                if (this.SetProperty(ref this.playerName, value))
                {
                    this.ErrorMessageVisibility = Visibility.Collapsed;
                    this.JoinGameCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private string playerName;

        public bool CanJoin
        {
            get { return this.canJoin && this.PlayerName != null && this.PlayerName.Length != 0; }
            set
            {
                if (this.SetProperty(ref this.canJoin, value))
                {
                    this.JoinGameCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private bool canJoin;

        public bool IsJoined
        {
            get { return this.isJoined; }
            set
            {
                if (this.SetProperty(ref this.isJoined, value))
                {
                    this.OnPropertyChanged(nameof(JoinVisibility));
                    this.OnPropertyChanged(nameof(GameUnderwayVisibility));
                    this.OnPropertyChanged(nameof(QuestionAvailableVisibility));
                }
                this.StateName = this.isJoined ? "stand by..." : "lobby";
            }
        }
        private bool isJoined;

        public Visibility ErrorMessageVisibility
        {
            get { return this.errorMessageVisibility; }
            set { this.SetProperty(ref this.errorMessageVisibility, value); }
        }
        private Visibility errorMessageVisibility = Visibility.Collapsed;

        public Visibility JoinVisibility
        {
            get { return !this.IsJoined ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility GameUnderwayVisibility
        {
            get { return this.IsJoined ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility QuestionAvailableVisibility
        {
            get { return this.IsJoined && this.CurrentQuestion != null ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Question CurrentQuestion
        {
            get { return this.currentQuestion; }
            set
            {
                if (this.IsJoined && this.SetProperty(ref this.currentQuestion, value))
                {
                    this.OnPropertyChanged(nameof(QuestionAvailableVisibility));
                    this.StateName = this.currentQuestion == null ? "lobby" : "make a selection...";
                    this.IsQuestionAnswered = false;
                    this.AnswerQuestionCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private Question currentQuestion;

        public DelegateCommand JoinGameCommand
        {
            get
            {
                return this.joinGameCommand ?? (this.joinGameCommand = new DelegateCommand(
                    () => this.ClientCommunicator.JoinGameAsync(this.PlayerName), 
                    () => this.CanJoin));
            }
        }
        private DelegateCommand joinGameCommand;

        public DelegateCommand LeaveGameCommand
        {
            get 
            { 
                return this.leaveGameCommand ?? (this.leaveGameCommand = new DelegateCommand(
                    () => {
                        this.ClientCommunicator.LeaveGameAsync(this.PlayerName);
                        this.IsJoined = false;
                        this.CurrentQuestion = null;
                    })); 
            }
        }
        private DelegateCommand leaveGameCommand;

        public DelegateCommand<string> AnswerQuestionCommand
        {
            get 
            { 
                return this.answerQuestionCommand ?? (this.answerQuestionCommand = new DelegateCommand<string>(
                    option => {
                        this.ClientCommunicator.AnswerQuestionAsync(PlayerName, Int32.Parse(option));
                        this.IsQuestionAnswered = true;
                        this.AnswerQuestionCommand.RaiseCanExecuteChanged();
                        this.StateName = "thank you";
                    }, 
                    option => !this.IsQuestionAnswered)); 
            }
        }
        private DelegateCommand<string> answerQuestionCommand;

    }
}