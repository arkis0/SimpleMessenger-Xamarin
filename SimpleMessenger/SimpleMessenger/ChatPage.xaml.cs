﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SimpleMessenger
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        private DatabaseService _databaseService;
        private Contact _contact;
        public ChatPage(Contact contact)
        {
            _contact = contact;
            this.Title = _contact.FullName;
            InitializeComponent();
            InitializeAsync();
            LoadMessages(contact);
        }
        private async void InitializeAsync()
        {
            _databaseService = new DatabaseService();
            await _databaseService.InitializeDatabase();
        }

        private async void LoadMessages(Contact contact)
        {
            var messages = await _databaseService.GetChatHistory(contact.Id);
            MessageListView.ItemsSource = messages;
            NoMessagesLabel.IsVisible = !messages.Any();
        }

        private async void EditMessageButton_Clicked(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var message = (Message)menuItem.CommandParameter;

            var result = await DisplayPromptAsync("Edycja wiadomości", "Wprowadź nową treść wiadomości", initialValue: message.Text);

            if (result != null)
            {
                message.Text = result;
                await _databaseService.UpdateMessage(message);
                LoadMessages(_contact);
            }
        }

        private async void DeleteMessageButton_Clicked(object sender, EventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var message = (Message)menuItem.CommandParameter;

            var confirm = await DisplayAlert("Usuwanie wiadomości", $"Czy jesteś pewien że chcesz usunąć tą wiadomość: {message.Text}?", "OK", "Anuluj");

            if (confirm)
            {
                await _databaseService.DeleteMessage(message);
                LoadMessages(_contact);
            }
        }

        private async void SendMessageButton_Clicked(object sender, EventArgs e)
        {
            var messageText = MessageEntry.Text;

            if (!string.IsNullOrEmpty(messageText))
            {
                var message = new Message
                {
                    Text = messageText,
                    SenderId = 0,
                    ReceiverId = _contact.Id,
                };

                await _databaseService.AddMessage(message);

                MessageEntry.Text = string.Empty;

                LoadMessages(_contact);
            }
        }
        private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }
    }
}