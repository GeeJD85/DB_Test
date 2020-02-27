using UnityEngine;
using SQLiteDatabase;
using System.Collections.Generic;
using UnityEngine.UI;

public class SQLite : MonoBehaviour
{
    public bool createNew;
    SQLiteDB testDB = SQLiteDB.Instance;

    public List<string> allIDs = new List<string>();
    public List<string> allNames = new List<string>();

    public int currentID = 0;
    public InputField playerInput;

    UserInfo_Manager userInfoManager;

    #region Enable/Disable
    private void OnEnable()
    {
        SQLiteEventListener.onError += OnError;
    }

    private void OnDisable()
    {
        SQLiteEventListener.onError -= OnError;
    }

    void OnError(string err)
    {
        Debug.LogWarning(err);
    }
    #endregion Enable/disable

    private void Start()
    {
        userInfoManager = GetComponent<UserInfo_Manager>();

        testDB.DBLocation = Application.persistentDataPath;
        testDB.DBName = "TestDB.db";
        Debug.Log("Database Directory : " + testDB.DBLocation);

        CreateDB();

        if(testDB.Exists)
            ConnectToDatabase();
    }

    void CreateDB()
    {
        if(createNew)
        {
            //Create a new blank database
            testDB.CreateDatabase(testDB.DBName, true);
        }
        else
        {
            //Connect to an already created Database
            testDB.ConnectToDefaultDatabase(testDB.DBName, false);
        }              

        //If no table named USERS exists, create it
        if (!testDB.IsTableExists("Users"))
            CreateTable();

        Refresh();
    }

    void ConnectToDatabase()
    {
        testDB.ConnectToDefaultDatabase(testDB.DBName, false);
        Refresh();
    }

    void CreateTable()
    {
        if(testDB.Exists)
        {
            //Create a table called Users
            DBSchema schema = new DBSchema("Users");

            //Create fields of the correct type
            schema.AddField("ID", SQLiteDB.DB_DataType.DB_INT, 0, false, true, true);
            schema.AddField("Name", SQLiteDB.DB_DataType.DB_STRING, 20, false, false, false);

            //Not necessary but can be used to return the table was successfuly created or not
            bool created = testDB.CreateTable(schema);
            Debug.Log(created);
        }
    }

    public void AddRow()
    {
        List<SQLiteDB.DB_DataPair> dataPairList = new List<SQLiteDB.DB_DataPair>();
        SQLiteDB.DB_DataPair data = new SQLiteDB.DB_DataPair();

        //First Field
        data.fieldName = "ID";
        data.value = currentID.ToString();
        dataPairList.Add(data);

        //Second Field
        data.fieldName = "Name";
        data.value = playerInput.text;
        dataPairList.Add(data);

        //Insert into Users table
        int i = testDB.Insert("Users", dataPairList);

        if(i > 0)
        {
            Debug.Log("Record inserted!");
            Refresh();
        }

        UserInfo userInfo = new UserInfo();
        userInfo.id = currentID;
        userInfo.userName = playerInput.text;
        userInfoManager.userInfoList.Add(userInfo);

        currentID++;
    }

    void Refresh()
    {
        allIDs.Clear();
        allNames.Clear();

        //Get all data from Users table
        DBReader reader = testDB.GetAllData("Users");

        while(reader != null && reader.Read())
        {
            allIDs.Add(reader.GetStringValue("ID"));
            allNames.Add(reader.GetStringValue("Name"));
        }
    }

    public void DisplayAllData()
    {
        for(int i=0; i<allIDs.Count; i++)
        {
            //iD.text = allIDs[i];
            //playerName.text = allNames[i];
        }
    }

    public void QueryUser(int id)
    {
        //iD.text = allIDs[id];
        //playerName.text = allNames[id];
    }

    //Use this to avoid any lock on database, otherwise restart editor or application after each run
    private void OnApplicationQuit()
    {
        //Release all resources used by database
        testDB.Dispose();
    }
}
