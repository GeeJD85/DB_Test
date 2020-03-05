using UnityEngine;
using SQLiteDatabase;
using System.Collections.Generic;
using UnityEngine.UI;

namespace GW.SQ
{
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

            if (testDB.Exists)
                ConnectToDatabase();
        }

        void CreateDB()
        {
            if (createNew)
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
            if (testDB.Exists)
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

            if (i > 0)
            {
                Debug.Log("Record inserted!");
                Refresh();
            }
        }

        //Update a row
        public void UpdateRow(string id, string name)
        {
            //List of data to be updated
            List<SQLiteDB.DB_DataPair> dataList = new List<SQLiteDB.DB_DataPair>();

            //Data to be updated
            SQLiteDB.DB_DataPair data = new SQLiteDB.DB_DataPair();
            data.fieldName = "Name";
            data.value = name;

            dataList.Add(data);

            //Row to be updated
            SQLiteDB.DB_ConditionPair condition = new SQLiteDB.DB_ConditionPair();
            condition.fieldName = "Id";
            condition.value = id;
            condition.condition = SQLiteDB.DB_Condition.EQUAL_TO;

            int i = testDB.Update("Users", dataList, condition);

            if (i > 0)
            {
                Debug.Log(i + " Record Updated!");
                Refresh();
            }
        }

        //Delete perticular id
        public void DeleteRow(string id)
        {
            SQLiteDB.DB_ConditionPair condition = new SQLiteDB.DB_ConditionPair();

            //Delete from Users where Id = id
            condition.fieldName = "Id";
            condition.value = id;
            condition.condition = SQLiteDB.DB_Condition.EQUAL_TO;

            int i = testDB.DeleteRow("Users", condition);
            if (i > 0)
            {
                Debug.Log(i + " Record Deleted!");
                Refresh();
            }
        }

        void Refresh()
        {
            userInfoManager.userInfoList.Clear();

            //Get all data from Users table
            DBReader reader = testDB.GetAllData("Users");

            while (reader != null && reader.Read())
            {
                UserInfo userInfo = new UserInfo();
                userInfo.id = (reader.GetIntValue("ID"));
                userInfo.userName = reader.GetStringValue("Name");

                userInfoManager.userInfoList.Add(userInfo);
                currentID = userInfo.id + 1;
            }
        }

        public void QueryUser(int id)
        {
            foreach(UserInfo userInfo in userInfoManager.userInfoList)
            {
                if (id == userInfo.id)
                {
                    userInfoManager.userInfoList.Clear();
                    userInfoManager.userInfoList.Add(userInfo);
                }
            }
        }

        //Use this to avoid any lock on database, otherwise restart editor or application after each run
        private void OnApplicationQuit()
        {
            //Release all resources used by database
            testDB.Dispose();
        }
    }
}