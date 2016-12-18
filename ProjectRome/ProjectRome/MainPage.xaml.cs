﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.System.RemoteSystems;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Windows.UI.Core;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ProjectRome
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ObservableCollection<RemoteSystem> deviceList = new ObservableCollection<RemoteSystem>();
        Dictionary<string, RemoteSystem> deviceMap = new Dictionary<string, RemoteSystem>();
        private RemoteSystemWatcher m_remoteSystemWatcher;
        public MainPage()
        {
            this.InitializeComponent();
            // store filter list
            List<IRemoteSystemFilter> listOfFilters = makeFilterList();
            // construct watcher with the list
            m_remoteSystemWatcher = RemoteSystem.CreateWatcher(listOfFilters);
        }

        private List<IRemoteSystemFilter> makeFilterList()
        {
            // construct an empty list
            List<IRemoteSystemFilter> localListOfFilters = new List<IRemoteSystemFilter>();

            // construct a discovery type filter that only allows "proximal" connections:
            RemoteSystemDiscoveryTypeFilter discoveryFilter = new RemoteSystemDiscoveryTypeFilter(RemoteSystemDiscoveryType.Proximal);


            // construct a device type filter that only allows desktop and mobile devices:
            // For this kind of filter, we must first create an IIterable of strings representing the device types to allow.
            // These strings are stored as static read-only properties of the RemoteSystemKinds class.
            List<String> listOfTypes = new List<String>();
            listOfTypes.Add(RemoteSystemKinds.Desktop);
            listOfTypes.Add(RemoteSystemKinds.Phone);

            // Put the list of device types into the constructor of the filter
            RemoteSystemKindFilter kindFilter = new RemoteSystemKindFilter(listOfTypes);


            // construct an availibility status filter that only allows devices marked as available:
            RemoteSystemStatusTypeFilter statusFilter = new RemoteSystemStatusTypeFilter(RemoteSystemStatusType.Available);


            // add the 3 filters to the listL
            localListOfFilters.Add(discoveryFilter);
            localListOfFilters.Add(kindFilter);
            localListOfFilters.Add(statusFilter);

            // return the list
            return localListOfFilters;
        }

        private void SearchByRemoteSystemWatcher()
        {

            // Subscribing to the event that will be raised when a new remote system is found by the watcher.
            m_remoteSystemWatcher.RemoteSystemAdded += RemoteSystemWatcher_RemoteSystemAdded;

            // Subscribing to the event that will be raised when a previously found remote system is no longer available.
            m_remoteSystemWatcher.RemoteSystemRemoved += RemoteSystemWatcher_RemoteSystemRemoved;

            // Subscribing to the event that will be raised when a previously found remote system is updated.
            m_remoteSystemWatcher.RemoteSystemUpdated += RemoteSystemWatcher_RemoteSystemUpdated;

            // Start the watcher.
            m_remoteSystemWatcher.Start();

            UpdateStatus("Searching for devices...", NotifyType.StatusMessage);
            DeviceListBox.Visibility = Visibility.Visible;
        }

        private async void RemoteSystemWatcher_RemoteSystemUpdated(RemoteSystemWatcher sender, RemoteSystemUpdatedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (deviceMap.ContainsKey(args.RemoteSystem.Id))
                {
                    deviceList.Remove(deviceMap[args.RemoteSystem.Id]);
                    deviceMap.Remove(args.RemoteSystem.Id);
                }
                deviceList.Add(args.RemoteSystem);
                deviceMap.Add(args.RemoteSystem.Id, args.RemoteSystem);
                UpdateStatus("Device updated with Id = " + args.RemoteSystem.Id, NotifyType.StatusMessage);
            });
        }

        private async void RemoteSystemWatcher_RemoteSystemRemoved(RemoteSystemWatcher sender, RemoteSystemRemovedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (deviceMap.ContainsKey(args.RemoteSystemId))
                {
                    deviceList.Remove(deviceMap[args.RemoteSystemId]);
                    UpdateStatus(deviceMap[args.RemoteSystemId].DisplayName + " removed.", NotifyType.StatusMessage);
                    deviceMap.Remove(args.RemoteSystemId);
                }
            });
        }

        private async void RemoteSystemWatcher_RemoteSystemAdded(RemoteSystemWatcher sender, RemoteSystemAddedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                deviceList.Add(args.RemoteSystem);
                deviceMap.Add(args.RemoteSystem.Id, args.RemoteSystem);
                UpdateStatus(string.Format("Found {0} devices.", deviceList.Count), NotifyType.StatusMessage);
            });
        }

    }
}
