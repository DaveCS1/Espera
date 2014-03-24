﻿using Espera.Core;
using Espera.Core.Settings;
using Espera.Core.Tests;
using Espera.View.ViewModels;
using NSubstitute;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Xunit;

namespace Espera.View.Tests
{
    public class YoutubeViewModelTest
    {
        [Fact]
        public void AvailableNetworkStartsSongSearch()
        {
            var isAvailable = new BehaviorSubject<bool>(false);
            var networkStatus = Substitute.For<INetworkStatus>();
            networkStatus.IsAvailable.Returns(isAvailable);

            var songFinder = Substitute.For<IYoutubeSongFinder>();
            songFinder.GetSongsAsync(Arg.Any<string>())
                .Returns(Task.FromResult((IReadOnlyList<YoutubeSong>)new List<YoutubeSong>()));

            using (var library = Helpers.CreateLibrary())
            {
                Guid token = library.LocalAccessControl.RegisterLocalAccessToken();
                var vm = new YoutubeViewModel(library, new ViewSettings(), new CoreSettings(), token, networkStatus, songFinder);

                isAvailable.OnNext(true);

                songFinder.ReceivedWithAnyArgs(1).GetSongsAsync(null);
            }
        }

        [Fact]
        public void SmokeTest()
        {
            var song1 = new YoutubeSong("www.youtube.com?watch=abcde", TimeSpan.Zero) { Title = "A" };
            var song2 = new YoutubeSong("www.youtube.com?watch=abcdef", TimeSpan.Zero) { Title = "B" };

            var songs = (IReadOnlyList<YoutubeSong>)new[] { song1, song2 }.ToList();

            var networkStatus = Substitute.For<INetworkStatus>();
            networkStatus.IsAvailable.Returns(Observable.Return(true));

            var songFinder = Substitute.For<IYoutubeSongFinder>();
            songFinder.GetSongsAsync(Arg.Any<string>()).Returns(Task.FromResult(songs));

            using (var library = Helpers.CreateLibrary())
            {
                Guid token = library.LocalAccessControl.RegisterLocalAccessToken();
                var vm = new YoutubeViewModel(library, new ViewSettings(), new CoreSettings(), token, networkStatus, songFinder);

                Assert.Equal(songs, vm.SelectableSongs.Select(x => x.Model).ToList());
                Assert.Equal(songs.First(), vm.SelectableSongs.First().Model);
                Assert.False(vm.IsSearching);
            }
        }

        [Fact]
        public void SongFinderExceptionSetsIsNetworkUnavailableToTrue()
        {
            var isAvailable = new BehaviorSubject<bool>(false);
            var networkStatus = Substitute.For<INetworkStatus>();
            networkStatus.IsAvailable.Returns(isAvailable);

            var songFinder = Substitute.For<IYoutubeSongFinder>();
            songFinder.GetSongsAsync(Arg.Any<string>()).Returns(x => { throw new Exception(); });

            using (var library = Helpers.CreateLibrary())
            {
                Guid token = library.LocalAccessControl.RegisterLocalAccessToken();
                var vm = new YoutubeViewModel(library, new ViewSettings(), new CoreSettings(), token, networkStatus, songFinder);

                var isNetworkUnavailable = vm.WhenAnyValue(x => x.IsNetworkUnavailable).CreateCollection();

                isAvailable.OnNext(true);

                Assert.Equal(new[] { true, false, true }, isNetworkUnavailable);
            }
        }

        [Fact]
        public async Task StartSearchSetsIsSearchingTest()
        {
            var networkStatus = Substitute.For<INetworkStatus>();
            networkStatus.IsAvailable.Returns(Observable.Return(false));

            var songFinder = Substitute.For<IYoutubeSongFinder>();
            songFinder.GetSongsAsync(Arg.Any<string>()).Returns(Task.FromResult((IReadOnlyList<YoutubeSong>)new List<YoutubeSong>()));

            using (var library = Helpers.CreateLibrary())
            {
                Guid token = library.LocalAccessControl.RegisterLocalAccessToken();
                var vm = new YoutubeViewModel(library, new ViewSettings(), new CoreSettings(), token, networkStatus, songFinder);

                var isSearching = vm.WhenAnyValue(x => x.IsSearching).CreateCollection();

                await vm.StartSearchAsync();

                Assert.Equal(new[] { false, true, false }, isSearching);
            }
        }

        [Fact]
        public void UnavailableNetworkSmokeTest()
        {
            var isAvailable = new BehaviorSubject<bool>(false);
            var networkStatus = Substitute.For<INetworkStatus>();
            networkStatus.IsAvailable.Returns(isAvailable);

            var songFinder = Substitute.For<IYoutubeSongFinder>();
            songFinder.GetSongsAsync(Arg.Any<string>()).Returns(Task.FromResult((IReadOnlyList<YoutubeSong>)new List<YoutubeSong>()));

            using (var library = Helpers.CreateLibrary())
            {
                Guid token = library.LocalAccessControl.RegisterLocalAccessToken();
                var vm = new YoutubeViewModel(library, new ViewSettings(), new CoreSettings(), token, networkStatus, songFinder);

                var isNetworkUnavailable = vm.WhenAnyValue(x => x.IsNetworkUnavailable).CreateCollection();

                isAvailable.OnNext(true);

                Assert.Equal(new[] { true, false }, isNetworkUnavailable);
            }
        }
    }
}