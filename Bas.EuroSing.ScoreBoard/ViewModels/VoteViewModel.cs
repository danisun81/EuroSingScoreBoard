﻿using Bas.EuroSing.ScoreBoard.Messages;
using Bas.EuroSing.ScoreBoard.Model;
using Bas.EuroSing.ScoreBoard.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bas.EuroSing.ScoreBoard.ViewModels
{
    internal class VoteViewModel : ViewModelBase
    {
        private IDataService dataService;

        public RelayCommand SettingsCommand { get; set; }

        public ObservableCollection<CountryListItemViewModel> Countries { get; set; }

        private CountryListItemViewModel countryIssuingVotes;

        public CountryListItemViewModel CountryIssuingVotes
        {
            get { return countryIssuingVotes; }
            set
            {
                Set(ref countryIssuingVotes, value);

                UpdateCountriesToVoteOn();
            }
        }

        private void UpdateCountriesToVoteOn()
        {
            CountriesToVoteOn.Clear();

            if (CountryIssuingVotes != null)
            {
                var votes = this.dataService.GetVotes(CountryIssuingVotes.Id);

                foreach (var vote in votes.OrderBy(v => v.ToCountry.Name))
                {
                    CountriesToVoteOn.Add(new CountryVoteViewModel(vote, this.dataService));
                }
            }
        }

        public ObservableCollection<CountryVoteViewModel> CountriesToVoteOn { get; set; }

        public VoteViewModel(IDataService dataService)
        {
            this.dataService = dataService;
            Messenger.Default.Register<CountriesUpdatedMessage>(this, (m) => 
            {
                UpdateCountries();
                UpdateCountriesToVoteOn();
            });

            SettingsCommand = new RelayCommand(() => MessengerInstance.Send(new GenericMessage<Message>(Message.ShowSettings)));
            CountriesToVoteOn = new ObservableCollection<CountryVoteViewModel>();
            Countries = new ObservableCollection<CountryListItemViewModel>();
            UpdateCountries();
        }

        private void UpdateCountries()
        {
            Countries.Clear();
            var allCountries = from c in dataService.GetAllCountries()
                               orderby c.Name
                               select new CountryListItemViewModel(c, this.dataService);

            foreach (var country in allCountries)
            {
                Countries.Add(country);
            }

            CountryIssuingVotes = null;
        }
    }
}
