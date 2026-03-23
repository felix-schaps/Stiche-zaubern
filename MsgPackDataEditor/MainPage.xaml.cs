using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using MessagePack;
using Stiche_Zaubern_MsgpLib;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace MsgPackDataEditor
{
    public sealed partial class MainPage : Page
    {
        private Dictionary<string, IDataInterpreter> knownTypes = new Dictionary<string, IDataInterpreter>()
        {
            {"Highscores", new DataInterpreter<List<HighscoreEntry>>()},
        };

        private IDataInterpreter selectedInterpreter;

        public Dictionary<TreeViewNode, Tuple<object, object>> treeDict = new Dictionary<TreeViewNode, Tuple<object, object>>();

        public MainPage()
        {
            this.InitializeComponent();
            TypeSelector.ItemsSource = knownTypes.Keys;
        }

        private void TypeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TypeSelector.SelectedItem is string typeName && knownTypes.TryGetValue(typeName, out var type))
            {
                selectedInterpreter = type;
                DataTree.RootNodes.Clear();
                treeDict.Clear();
            }
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            DataTree.RootNodes.Clear();
            if (selectedInterpreter == null) return;

            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".dat");
            var file = await picker.PickSingleFileAsync();
            if (file == null) return;
            var stream = await file.OpenStreamForReadAsync();
            selectedInterpreter.Load(stream);
            selectedInterpreter.Render(DataTree, treeDict);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedInterpreter == null) return;

            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add("Datei", new List<string>() { ".dat" });
            picker.SuggestedFileName = "data";
            var file = await picker.PickSaveFileAsync();
            if (file == null) return;

            using (var stream = await file.OpenStreamForWriteAsync())
            {
                selectedInterpreter.Save(stream);
            }
        }

        private void DataTree_SelectionChanged(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            EditorPanel.Children.Clear();
            var node = args.InvokedItem as TreeViewNode;
            var cont = treeDict.GetValueOrDefault(node);
            var (target, arg) = cont;
            if (selectedInterpreter.IsList(target) && arg==null)
            {
                // Liste ausgewaehlt, Add-Button hinzufuegen
                var addButton = new Button { Content = "➕ New Element" };
                addButton.Click += (s, e) =>
                {
                    var elementType = target.GetType().GetGenericArguments().FirstOrDefault();
                    if (elementType != null)
                    {
                        var newItem = Activator.CreateInstance(elementType);
                        ((IList)target).Add(newItem);
                        DataTree.RootNodes.Clear();
                        selectedInterpreter.Render(DataTree,treeDict);
                    }
                };
                EditorPanel.Children.Add(addButton);
            }
            else if (arg is PropertyInfo) {
                var prop = arg as PropertyInfo;
                BuildEditor(target, prop);
            }
            else if(!selectedInterpreter.IsList(target) && selectedInterpreter.IsList(arg))
            {
                // Listen-Element ausgewaehlt
                BuildEditor(target);

                var remButton = new Button { Content = "Remove Element" };
                remButton.Click += (s, e) =>
                {
                        ((IList)arg).Remove(target);
                        DataTree.RootNodes.Clear();
                        selectedInterpreter.Render(DataTree, treeDict);
                };
                EditorPanel.Children.Add(remButton);
            }
        }

        private void BuildEditor(object obj, PropertyInfo prop)
        {
            var value = prop.GetValue(obj);
            var label = new TextBlock { Text = prop.Name, Margin = new Thickness(0, 10, 0, 5) };
            EditorPanel.Children.Add(label);
            var type = prop.PropertyType;
            var box = new TextBox { Text = value?.ToString() ?? "", Width = 200 };
            box.Tag = (obj, prop);
            box.TextChanged += (s, e) =>
            {
                
                try
                {
                    object converted = Convert.ChangeType(box.Text,type);
                    prop.SetValue(obj, converted);
                }
                catch { }
            };
            EditorPanel.Children.Add(box);
        }


        private void BuildEditor(object obj)
        {
            var type = obj.GetType();
            foreach (var prop in type.GetProperties())
            {
                BuildEditor(obj, prop);
            }
        }
    }

    public interface IDataInterpreter
    {
        void Load(Stream file);
        void Save(Stream file);
        void Render(TreeView control, Dictionary<TreeViewNode, Tuple<object, object>> treeDict);
        bool IsList(object obj);
    }
    public class DataInterpreter<T> : IDataInterpreter where T: class
    {
        private T data;
        private TreeView root;
        Dictionary<TreeViewNode, Tuple<object, object>> treeDict;

        public void Load(Stream stream)
        {
            data = MessagePackSerializer.Deserialize<T>(stream);
        }

        public void Save(Stream stream)
        {
            MessagePackSerializer.Serialize(stream, data);
        }

        public void Render(TreeView control, Dictionary<TreeViewNode, Tuple<object, object>> treeDict)
        {
            root = control;
            this.treeDict = treeDict;
            RenderTree(data, null, control, null);
        }

        public bool IsList(object obj)
        {
            var type = obj.GetType();
            return typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string);
        }

        private void RenderTree(object obj, TreeViewNode parentTree, object parentObj, object arg)
        {
            if (obj == null) return;
            var type = obj.GetType();

            if (IsList(obj))
            {
                var listNode = new TreeViewNode { Content = $"List<{type.GenericTypeArguments.FirstOrDefault()?.Name ?? "?"}>" };
                treeDict.Add(listNode, new Tuple<object,object>(obj, null));
                if (parentTree == null) root.RootNodes.Add(listNode);
                else parentTree.Children.Add(listNode);

                foreach (var item in (IEnumerable)obj)
                {
                    RenderTree(item, listNode, obj, null);
                }
            }
            else if (type.IsClass && type != typeof(string))
            {
                var objectNode = new TreeViewNode { Content = type.Name };
                treeDict.Add(objectNode, new Tuple<object, object>(obj, parentObj));
                if (parentTree == null) root.RootNodes.Add(objectNode);
                else parentTree.Children.Add(objectNode);

                foreach (var prop in type.GetProperties())
                {
                    var value = prop.GetValue(obj);
                    RenderTree(value, objectNode, obj, prop);
                }
            }
            else
            {            
                var leafNode = new TreeViewNode { Content = $"{((PropertyInfo)arg).Name} -> {type.Name}: {obj}" };
                treeDict.Add(leafNode, new Tuple<object, object>(parentObj,arg));
                parentTree?.Children.Add(leafNode);
            }
        }

    }
};
