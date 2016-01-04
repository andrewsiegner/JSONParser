using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



namespace JSONConvert
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Global Scope TreeViewItem
        /// Serves as a root item for each object
        /// Given a list of JSON objects as the root object this item is cleared and repurposed
        /// for each object
        /// Given a single JSON object this item is only used once as the root item 
        /// </summary>  
        public TreeViewItem tempItem;

        public MainWindow()
        {
            InitializeComponent();
            rtbIn.Document.Blocks.Clear();
            //Initialize the Global Scope TreeViewItem and give it a header
            tempItem = new TreeViewItem();
            tempItem.Header = "Root";
        }
        /// <summary>
        /// Click event handler for the Convert button
        /// Converts the JSON string into an ExpandoObject or List of ExpandoObject and 
        /// Calls the recursive processTreeView function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            
            var text2 = StringFromRTB(rtbIn);

            //Initialize empty list of ExpandoObjects to be displayed
            List<ExpandoObject> obj = new List<ExpandoObject>();
            try
            {
                //If JSON supplied is a list of objects run the jsonListConvert function
                //to directly populate obj
                obj = jsonListConvert(text2);
            }
            catch (Exception ex)
            {
                //If the JSON string is not a list of objects the above try block fails
                //try to convert the object as a single object and store as the 
                //only object in the ExpandoObject list
                try
                {
                    obj.Add(jsonconvert(text2));
                }
                catch (Exception exc)
                {
                    //If neither operation is successful, ask the user to check JSON text
                    MessageBox.Show("JSON Conversion error, ensure text is a valid JSON object.  If the JSON is valid, contact support at thisisafakeemail@shh.com. \n\nMore Info: " + exc.Message );
                }
            }
            finally
            {
                //Ensure that the list has values in it
                if (obj != null)
                {
                    //Iterate through the list of ExpandoObjects, calling the recursive processTreeView
                    //function for each one
                    foreach (ExpandoObject objet in obj)
                    {
                        //Ensure each item in the list has value
                        if (objet != null)
                        {
                            //Call recursive function which processes a tree view
                            //Gives each object as the first parameter and tells it that 
                            //its parent object is the global scope root object
                            processTreeView(objet, tempItem);
                            //Add the now populated root object to the tree view items collection
                            treeViewOut.Items.Add(tempItem);
                            //Clear the tempItem object so that it can be reused for additional objects
                            tempItem = new TreeViewItem();
                            tempItem.Header = "Root";
                        }
                    }
                }
            }
        }

        private string StringFromRTB(RichTextBox rtb)
        {
            TextRange textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            return textRange.Text;
        }

        /// <summary>
        /// Takes a Json string representing a single object and converts it into an ExpandoObject
        /// </summary>
        /// <param name="json">a JSON String representing a JSON object </param>
        /// <returns>an ExpandoObject representing the object in the JSON string</returns>
        private ExpandoObject jsonconvert(string json)
        {
            return JsonConvert.DeserializeObject<ExpandoObject>(json);
        }
        /// <summary>
        /// Takes a JSON string representing a list of objects and converts it into a list of ExpandoObject
        /// </summary>
        /// <param name="json">a string representing a list of json objects</param>
        /// <returns>a list of ExpandoObject</returns>
        private List<ExpandoObject> jsonListConvert(string json)
        {
            return JsonConvert.DeserializeObject<List<ExpandoObject>>(json);
        }
        /// <summary>
        /// Closes the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// A recursive function which iterates through the tree of any object and by taking
        /// a parameter indicating the intended parent object and using if statements and type
        /// information to transform the global TreeViewItem into an easily readable representation of 
        /// the object
        /// </summary>
        /// <param name="obj">A dynamically typed object to be examined and processed by the recursive function</param>
        /// <param name="parent">the parent treeview item the object should be added to</param>
        private void processTreeView(Object obj, TreeViewItem parent)
        {
            //Checks if the object is an ExpandoObject
            if (obj.GetType() == typeof(System.Dynamic.ExpandoObject))
            {
                
                //If so, treat the object as an IDictionary and iterate through its properties 
                foreach (var property in (IDictionary<String, Object>)obj)
                {
                    //Check if the object has a name property, if so give the parents header that value
                    if (property.Key == "name" || property.Key == "Name" || property.Key == "NAME")
                    {
                        parent.Header = property.Value.ToString();
                        
                        
                    }
                    //Ensure that only properties with values are processed
                    if (property.Value != null)
                    {
                        //If the item is a nested object, give it a new TreeViewItem
                        //get the header text from its key value
                        //adds the treeview to its logical parent and calls this function
                        //given the properts value to process and the new object as the new parent
                        if (property.Value.GetType() == typeof(ExpandoObject))
                        {
                            TreeViewItem tempObj = new TreeViewItem();
                            tempObj.Header = property.Key.ToString();
                            parent.Items.Add(tempObj);
                            processTreeView(property.Value, tempObj);
                        }
                        else if (property.Value.GetType() == typeof(List<string>))
                        {
                            //If its a list of strings, create a temporary treeview item, add the string
                            //and add the treeviewitem it to its logical parent
                            foreach (string g in (List<string>)property.Value)
                            {
                                TreeViewItem stringListTemp = new TreeViewItem();
                                stringListTemp.Items.Add(g);
                                parent.Items.Add(stringListTemp);
                            }
                        }

                        else if (property.Value.GetType() == typeof(List<Object>))
                        {
                            //If the Object is a list of objects, process them appropriately
                            TreeViewItem tvi = new TreeViewItem();
                            tvi.Header = property.Key.ToString();
                            //Iterate through the objects and handle them depending on their type
                            foreach (Object o in (List<Object>)property.Value)
                            {
                                if (o.GetType() == typeof(ExpandoObject))
                                {
                                    //Initialize a string to represent a header for the object
                                    string headerText = property.Key + " item";
                                    //If it finds a name property in the object, give the header text that 
                                    //value, if not the header will be the parents name with item concatenated
                                    foreach (var nameCheck in (IDictionary<String, Object>)o)
                                    {
                                        if (nameCheck.Key == "name" || nameCheck.Key == "Name")
                                        {
                                            
                                            headerText = nameCheck.Value.ToString();
                                        }
                                    }
                                    //Create a new treeview item, give it its header text
                                    TreeViewItem xTemp = new TreeViewItem();
                                    xTemp.Header = headerText;
                                    //process each item with this new temporary tree view item as the parent
                                    processTreeView(o, xTemp);
                                    //add the processed temporary item to its logical parent
                                    tvi.Items.Add(xTemp);
                                }
                                //If its a list of strings, add each one to its parent
                                else if (o.GetType() == typeof(List<string>))
                                {
                                    foreach (String teste in (List<string>)o)
                                    {
                                        tvi.Items.Add(teste.ToString());
                                    }
                                }
                                //Pretty important this is here and I knew why before but now I don't
                                else if (o.GetType() == typeof(List<Object>))
                                {
                                    foreach (Object teste in (List<Object>)o)
                                    {
                                        tvi.Items.Add(teste.ToString());
                                    }
                                }
                                    //If the data is any other type, like a number or character
                                    //It is simply added as a string to its parent
                                else
                                {
                                    tvi.Items.Add(o);
                                }
                            }
                            //Adds whatever tree view item the object at hand was processed into to 
                            //the parent
                            parent.Items.Add(tvi);
                        }
                        else
                        {
                            //If the object passed into the function was a simple object such as a 
                            //string or number, simply add it to its parent                           
                            TreeViewItem bottom = new TreeViewItem();
                            bottom.Items.Add(property.Value);
                            bottom.Header = property.Key;
                            parent.Items.Add(bottom);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Clears the treeview of all previously decoded items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            treeViewOut.Items.Clear();
        }

        private void buttonClearText_Click(object sender, RoutedEventArgs e)
        {
            rtbIn.Document.Blocks.Clear();
        }
    }
}

