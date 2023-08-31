using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ProtoBuf;
using Serilog;

namespace DNSSwitcher;

public partial class MainWindow : Window
{
    private bool _custom;

    private List<Dns> _dnsList;

    //private Settings settings;
    private Dns? _selectedDns;

    public MainWindow()
    {
        InitializeSeriLog();
        InitializeComponent();
        Load();
    }

    private void InitializeSeriLog()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/DNSSwitcher.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        Log.Information("Start!");
    }

    private void Load()
    {
        #region Load DNS

        Log.Information("Loading dns s");
        if (File.Exists("Dns.proto"))
        {
            Log.Information("Dns file exists.");
            using (var file = File.OpenRead("Dns.proto"))
            {
                _dnsList = Serializer.Deserialize<List<Dns>>(file);
            }
        }
        else
        {
            Log.Information("Dns file doesnt exist. creating it...");
            _dnsList = new List<Dns>();
        }

        RefreshDnsListCb();

        #endregion

        #region Load Settings

        Log.Information("Skipping loading settings. its not implemented yet lol");
        //Log.Information("Loading settings");
        //if (File.Exists("Settings.proto"))
        //{
        //    Log.Information("Settings file exists");
        //    using (var file = File.OpenRead("Settings.proto"))
        //        settings = Serializer.Deserialize<Settings>(file);
        //}
        //else
        //{
        //    Log.Information("Setting file doesnt exist. skipping...");
        //    settings = new Settings(true);
        //}
//

        #endregion //Not used for now

        #region Check For Update

        Log.Information("Skipping checking for updates. Not implemented yet lol");
        //TODO:AutoUpdate

        #endregion

        StatusL.Content = string.Empty;
    }

    private void RefreshDnsListCb()
    {
        try
        {
            _selectedDns = null;
            _custom = false;
            DnsListCb.SelectedItem = null;
            List<ComboBoxItem> dnsListCbL = new();
            foreach (var dns in _dnsList)
                dnsListCbL.Add(new ComboBoxItem { Content = dns.Name, Tag = dns });
            dnsListCbL.Add(new ComboBoxItem { Content = "Custom" });
            DnsListCb.Items = dnsListCbL;
            DnsListCb_OnSelectionChanged(DnsListCb, null);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to refresh dns list combobox");
            StatusL.Content = "Failed to refresh dns list combobox";
            Thread.Sleep(1000);
            StatusL.Content = "App crashed...";
            Thread.Sleep(500);
            throw;
        }
    }

    private void DnsListCb_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DnsListCb.SelectedIndex < _dnsList.Count && DnsListCb.SelectedIndex >= 0)
        {
            SetDnsB.IsVisible = true;
            Dns1PrefixL.IsVisible = true;
            Dns2PrefixL.IsVisible = true;
            DnsActionB.IsVisible = true;

            Dns1L.IsVisible = true;
            Dns2L.IsVisible = true;
            Dns1Tb.IsVisible = false;
            Dns2Tb.IsVisible = false;
            DnsActionB.Content = "Remove from list";

            _custom = false;
            _selectedDns = _dnsList[DnsListCb.SelectedIndex];
            Dns1L.Content = _selectedDns.Dns1;
            Dns2L.Content = _selectedDns.Dns2;
        }
        else if (DnsListCb.SelectedIndex >= _dnsList.Count)
        {
            SetDnsB.IsVisible = true;
            Dns1PrefixL.IsVisible = true;
            Dns2PrefixL.IsVisible = true;
            DnsActionB.IsVisible = true;

            Dns1L.IsVisible = false;
            Dns2L.IsVisible = false;
            Dns1Tb.IsVisible = true;
            Dns2Tb.IsVisible = true;
            DnsActionB.Content = "Add to list";

            _custom = true;
            _selectedDns = new Dns("Custom", Dns1Tb.Text, Dns2Tb.Text);
        }
        else
        {
            SetDnsB.IsVisible = false;
            Dns1PrefixL.IsVisible = false;
            Dns2PrefixL.IsVisible = false;
            DnsActionB.IsVisible = false;

            Dns1L.IsVisible = false;
            Dns2L.IsVisible = false;
            Dns1Tb.IsVisible = false;
            Dns2Tb.IsVisible = false;

            _custom = false;
            _selectedDns = null;
        }
    }

    private void Dns1Tb_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_custom)
            _selectedDns.Dns1 = Dns1Tb.Text;
    }

    private void Dns2Tb_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_custom)
            _selectedDns.Dns2 = Dns2Tb.Text;
    }

    private void GetDDnsB_OnClick(object? sender, RoutedEventArgs e)
    {
        HttpClient httpClient = new();
        Log.Information("Trying to get dnss from server");
        StatusL.Content = "Downloading default dnss...";
        try
        {
            _dnsList = httpClient
                .GetFromJsonAsync<List<Dns>>("https://raw.githubusercontent.com/XSilverTH/DNSSwitcher/main/DnsList.json").Result;
        }
        catch (Exception ex)
        {
            Log.Error(ex + "\nFailed to get dnss from server");
            StatusL.Content = "Failed to download default dnss";
            return;
        }

        ApplyChangesToDnsList();
        Log.Information("Successfully got dnss from server");
        StatusL.Content = string.Empty;
    }

    private void ApplyChangesToDnsList()
    {
        RefreshDnsListCb();
        try
        {
            using (var file = File.Create("Dns.proto"))
            {
                Serializer.Serialize(file, _dnsList);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex + "\nFailed to save dnss to file");
        }
    }

    private void SetDnsB_OnClick(object? sender, RoutedEventArgs e)
    {
        Log.Information("Trying to set dns");
        StatusL.Content = "Setting dns...";
        if (_selectedDns == null)
        {
            Log.Information("No dns selected");
            StatusL.Content = "No dns selected";
            return;
        }

        try
        {
            DnsService.SetDNS(_selectedDns);
        }
        catch (Exception ex)
        {
            Log.Error(ex + "\nFailed to set dns");
            StatusL.Content = "Failed to set dns";
            return;
        }

        Log.Information("Set dns");
        StatusL.Content = string.Empty;
    }

    private void ResetDnsB_OnClick(object? sender, RoutedEventArgs e)
    {
        Log.Information("Trying to reset dns");
        StatusL.Content = "Resetting dns...";
        try
        {
            DnsService.ResetDNS();
        }
        catch (Exception ex)
        {
            Log.Error(ex + "\nFailed to reset dns");
            StatusL.Content = "Failed to reset dns";
            return;
        }

        StatusL.Content = string.Empty;
    }

    private void DnsActionB_OnClick(object? sender, RoutedEventArgs e)
    {
        StatusL.Content = "Wait...";
        if (!_custom)
        {
            Log.Information("Trying to remove a dns to list");
            if (_selectedDns == null)
            {
                Log.Information("No dns selected");
                StatusL.Content = "No dns selected";
                return;
            }

            Log.Information($"Trying to remove '{_selectedDns.Name}' from dnslist");
            try
            {
                _dnsList.Remove(_selectedDns);
            }
            catch (Exception ex)
            {
                Log.Error(ex + "\nFailed to remove dns from list");
                StatusL.Content = "Failed to remove dns from list";
                return;
            }

            ApplyChangesToDnsList();
            Log.Information("Removed a dns from dnslist");
            StatusL.Content = string.Empty;
            return;
        }

        Log.Information("Trying to add a dns to list");
        Log.Information($"First step. Dns1:{Dns1Tb.Text}, Dns2:{Dns2Tb.Text}");
        DnsActionB.IsVisible = false;
        DnsActionWTb.IsVisible = true;
        DnsActionWB.IsVisible = true;

        StatusL.Content = string.Empty;
    }

    private void DnsActionWB_OnClick(object? sender, RoutedEventArgs e)
    {
        Log.Information("Trying to add a dns to list");
        StatusL.Content = "Adding a dns to dns list";
        Log.Information($"Second step. Name:{DnsActionWTb.Text}, Dns1:{Dns1Tb.Text}, Dns2:{Dns2Tb.Text}");
        try
        {
            _dnsList.Add(new Dns(DnsActionWTb.Text, Dns1Tb.Text, Dns2Tb.Text));
            ApplyChangesToDnsList();
        }
        catch (Exception ex)
        {
            Log.Error(ex + "\nFailed to add dns to list");
            StatusL.Content = "Failed to add dns to list";
            return;
        }
        finally
        {
            DnsActionB.IsVisible = true;
            DnsActionWTb.IsVisible = false;
            DnsActionWB.IsVisible = false;
        }

        Log.Information($"Added {DnsActionWTb.Text} to dns list");
        StatusL.Content = string.Empty;
    }
}
