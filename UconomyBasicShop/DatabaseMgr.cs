using I18N.West;
using MySql.Data.MySqlClient;
using Rocket.Logging;
using Rocket.RocketAPI;
using System;
using unturned.ROCKS.Uconomy;

namespace UconomyBasicShop
{
    public class DatabaseMgr
    {
        // The base code for this class comes from Uconomy itself.

        internal DatabaseMgr()
        {
            CP1250 cP1250 = new CP1250();
            this.CheckSchema();
        }

        internal void CheckSchema()
        {
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("show tables like '", UconomyBasicShop.Instance.Configuration.ItemShopTableName, "'");
                mySqlConnection.Open();
                if (mySqlCommand.ExecuteScalar() == null)
                {
                    mySqlCommand.CommandText = string.Concat("CREATE TABLE `", UconomyBasicShop.Instance.Configuration.ItemShopTableName, "` (`id` int(6) NOT NULL,`itemname` varchar(32) NOT NULL,`cost` decimal(15,2) NOT NULL DEFAULT '20.00',PRIMARY KEY (`id`)) ");
                    mySqlCommand.ExecuteNonQuery();
                }
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("show tables like '", UconomyBasicShop.Instance.Configuration.VehicleShopTableName, "'");
                mySqlConnection.Open();
                if (mySqlCommand.ExecuteScalar() == null)
                {
                    mySqlCommand.CommandText = string.Concat("CREATE TABLE `", UconomyBasicShop.Instance.Configuration.VehicleShopTableName, "` (`id` int(6) NOT NULL,`vehiclename` varchar(32) NOT NULL,`cost` decimal(15,2) NOT NULL DEFAULT '100.00',PRIMARY KEY (`id`)) ");
                    mySqlCommand.ExecuteNonQuery();
                }
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
        }

        private MySqlConnection createConnection()
        {
            MySqlConnection mySqlConnection = null;
            try
            {
                if (Uconomy.Instance.Configuration.DatabasePort == 0)
                {
                    Uconomy.Instance.Configuration.DatabasePort = 3306;
                }
                mySqlConnection = new MySqlConnection(string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", new object[] { 
                    Uconomy.Instance.Configuration.DatabaseAddress, 
                    Uconomy.Instance.Configuration.DatabaseName, 
                    Uconomy.Instance.Configuration.DatabaseUsername, 
                    Uconomy.Instance.Configuration.DatabasePassword,
                    Uconomy.Instance.Configuration.DatabasePort}));
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
            return mySqlConnection;
        }

        public bool AddItem(int id, string name, decimal cost, bool change)
        {
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                if (!change)
                {
                    mySqlCommand.CommandText = string.Concat(
                        new string[] { 
                            "Insert into `", 
                            UconomyBasicShop.Instance.Configuration.ItemShopTableName, 
                            "` (`id`, `itemname`, `cost`) VALUES ('",
                            id.ToString(),
                            "', '",
                            name,
                            "', '",
                            cost.ToString(),
                            "');" 
                        });
                }
                else
                {
                    mySqlCommand.CommandText = string.Concat(
                        new string[] { 
                            "update `",
                            UconomyBasicShop.Instance.Configuration.ItemShopTableName,
                            "` set itemname='",
                            name,
                            "', cost='",
                            cost.ToString(),
                            "' where id='",
                            id.ToString(),
                            "';" 
                        });
                }
                mySqlConnection.Open();
                mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
                return true;
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
                return false;
            }
        }

        public bool AddVehicle(int id, string name, decimal cost, bool change)
        {
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                if (!change)
                {
                    mySqlCommand.CommandText = string.Concat(
                        new string[] { 
                            "Insert into `",
                            UconomyBasicShop.Instance.Configuration.VehicleShopTableName,
                            "` (`id`, `vehiclename`, `cost`) VALUES ('",
                            id.ToString(),
                            "', '",
                            name,
                            "', '",
                            cost.ToString(),
                            "');" 
                        });
                }
                else
                {
                    mySqlCommand.CommandText = string.Concat(
                        new string[] { 
                            "update `",
                            UconomyBasicShop.Instance.Configuration.VehicleShopTableName,
                            "` set vehiclename='",
                            name,
                            "', cost='",
                            cost.ToString(),
                            "' where id='",
                            id.ToString(),
                            "';" 
                        });
                }
                mySqlConnection.Open();
                mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
                return true;
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
                return false;
            }
        }

        public decimal GetItemCost(int id)
        {
            decimal num = new decimal(0);
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat(new string[] { 
                    "select `cost` from `",
                    UconomyBasicShop.Instance.Configuration.ItemShopTableName,
                    "` where `id` = '",
                    id.ToString(),
                    "';" 
                });
                mySqlConnection.Open();
                object obj = mySqlCommand.ExecuteScalar();
                if (obj != null)
                {
                    decimal.TryParse(obj.ToString(), out num);
                }
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
            return num;
        }

        public decimal GetVehicleCost(int id)
        {
            decimal num = new decimal(0);
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat(new string[] { 
                    "select `cost` from `",
                    UconomyBasicShop.Instance.Configuration.VehicleShopTableName, 
                    "` where `id` = '", 
                    id.ToString(), 
                    "';" 
                });
                mySqlConnection.Open();
                object obj = mySqlCommand.ExecuteScalar();
                if (obj != null)
                {
                    decimal.TryParse(obj.ToString(), out num);
                }
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
            return num;
        }

        public bool DeleteItem(int id)
        {
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat(
                    new string[] { 
                        "delete from `",
                        UconomyBasicShop.Instance.Configuration.ItemShopTableName, 
                        "` where id='", 
                        id.ToString(), 
                        "';" 
                    });
                mySqlConnection.Open();
                mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
                return true;
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
                return false;
            }
        }

        public bool DeleteVehicle(int id)
        {
            try
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat(
                    new string[] { 
                        "delete from `", 
                        UconomyBasicShop.Instance.Configuration.VehicleShopTableName,
                        "` where id='", 
                        id.ToString(), 
                        "';" 
                    });
                mySqlConnection.Open();
                mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
                return true;
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
                return false;
            }
        }
    }
}