using System;
using I18N.West;
using MySql.Data.MySqlClient;
using Rocket.API.Logging;
using Rocket.Core.Logging;

namespace ZaupShop
{
    public class DatabaseMgr
    {
        private readonly ZaupShop _zaupShop;
        private readonly ILogger _logger;

        // The base code for this class comes from Uconomy itself.

        internal DatabaseMgr(ILogger logger, ZaupShop zaupShop)
        {
            _zaupShop = zaupShop;
            _logger = logger;
            CP1250 cP1250 = new CP1250();
            CheckSchema();
        }

        private void CheckSchema()
        {
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("show tables like '", ZaupShop.Instance.ConfigurationInstance.ItemShopTableName, "'");
                mySqlConnection.Open();
                if (mySqlCommand.ExecuteScalar() == null)
                {
                    mySqlCommand.CommandText = string.Concat("CREATE TABLE `", ZaupShop.Instance.ConfigurationInstance.ItemShopTableName, "` (`id` int(6) NOT NULL,`itemname` varchar(32) NOT NULL,`cost` decimal(15,2) NOT NULL DEFAULT '20.00',`buyback` decimal(15,2) NOT NULL DEFAULT '0.00',PRIMARY KEY (`id`)) ");
                    mySqlCommand.ExecuteNonQuery();
                }
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                _logger.LogError(null, exception);
            }
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("show tables like '", ZaupShop.Instance.ConfigurationInstance.VehicleShopTableName, "'");
                mySqlConnection.Open();
                if (mySqlCommand.ExecuteScalar() == null)
                {
                    mySqlCommand.CommandText = string.Concat("CREATE TABLE `", ZaupShop.Instance.ConfigurationInstance.VehicleShopTableName, "` (`id` int(6) NOT NULL,`vehiclename` varchar(32) NOT NULL,`cost` decimal(15,2) NOT NULL DEFAULT '100.00',PRIMARY KEY (`id`)) ");
                    mySqlCommand.ExecuteNonQuery();
                }
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                _logger.LogError(null, exception);
            }
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("show columns from `", ZaupShop.Instance.ConfigurationInstance.ItemShopTableName, "` like 'buyback'");
                mySqlConnection.Open();
                if (mySqlCommand.ExecuteScalar() == null)
                {
                    mySqlCommand.CommandText = string.Concat("ALTER TABLE `", ZaupShop.Instance.ConfigurationInstance.ItemShopTableName, "` ADD `buyback` decimal(15,2) NOT NULL DEFAULT '0.00'");
                    mySqlCommand.ExecuteNonQuery();
                }
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                _logger.LogError(null, exception);
            }
        }

        public MySqlConnection CreateConnection()
        {
            MySqlConnection mySqlConnection = null;
            try
            {
                if (_zaupShop.ConfigurationInstance.DatabasePort == 0)
                {
                    _zaupShop.ConfigurationInstance.DatabasePort = 3306;
                    _zaupShop.SaveConfiguration();
                }
                mySqlConnection = new MySqlConnection(string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", _zaupShop.ConfigurationInstance.DatabaseAddress, _zaupShop.ConfigurationInstance.DatabaseName, _zaupShop.ConfigurationInstance.DatabaseUsername, _zaupShop.ConfigurationInstance.DatabasePassword, _zaupShop.ConfigurationInstance.DatabasePort));
            }
            catch (Exception exception)
            {
                _logger.LogError(null, exception);
            }
            return mySqlConnection;
        }

        public bool AddItem(int id, string name, decimal cost, bool change)
        {
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                if (!change)
                {
                    mySqlCommand.CommandText = string.Concat("Insert into `", ZaupShop.Instance.ConfigurationInstance.ItemShopTableName, "` (`id`, `itemname`, `cost`) VALUES ('", id.ToString(), "', '", name, "', '", cost.ToString(), "');");
                }
                else
                {
                    mySqlCommand.CommandText = string.Concat("update `", ZaupShop.Instance.ConfigurationInstance.ItemShopTableName, "` set itemname='", name, "', cost='", cost.ToString(), "' where id='", id.ToString(), "';");
                }
                mySqlConnection.Open();
                int affected = mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
                if (affected > 0)
                {
                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                _logger.LogError(null, exception);
                return false;
            }
        }

        public bool AddVehicle(int id, string name, decimal cost, bool change)
        {
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                if (!change)
                {
                    mySqlCommand.CommandText = string.Concat("Insert into `", ZaupShop.Instance.ConfigurationInstance.VehicleShopTableName, "` (`id`, `vehiclename`, `cost`) VALUES ('", id.ToString(), "', '", name, "', '", cost.ToString(), "');");
                }
                else
                {
                    mySqlCommand.CommandText = string.Concat("update `", ZaupShop.Instance.ConfigurationInstance.VehicleShopTableName, "` set vehiclename='", name, "', cost='", cost.ToString(), "' where id='", id.ToString(), "';");
                }
                mySqlConnection.Open();
                int affected = mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
                if (affected > 0)
                {
                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                _logger.LogError(null, exception);
                return false;
            }
        }

        public decimal GetItemCost(int id)
        {
            decimal num = new decimal(0);
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("select `cost` from `", ZaupShop.Instance.ConfigurationInstance.ItemShopTableName, "` where `id` = '", id.ToString(), "';");
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
                _logger.LogError(null, exception);
            }
            return num;
        }

        public decimal GetVehicleCost(int id)
        {
            decimal num = new decimal(0);
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("select `cost` from `", ZaupShop.Instance.ConfigurationInstance.VehicleShopTableName, "` where `id` = '", id.ToString(), "';");
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
                _logger.LogError(null, exception);
            }
            return num;
        }

        public bool DeleteItem(int id)
        {
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("delete from `", ZaupShop.Instance.ConfigurationInstance.ItemShopTableName, "` where id='", id.ToString(), "';");
                mySqlConnection.Open();
                int affected = mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
                if (affected > 0)
                {
                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                _logger.LogError(null, exception);
                return false;
            }
        }

        public bool DeleteVehicle(int id)
        {
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("delete from `", ZaupShop.Instance.ConfigurationInstance.VehicleShopTableName, "` where id='", id.ToString(), "';");
                mySqlConnection.Open();
                int affected = mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
                if (affected > 0)
                {
                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                _logger.LogError(null, exception);
                return false;
            }
        }

        public bool SetBuyPrice(int id, decimal cost)
        {
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("update `", ZaupShop.Instance.ConfigurationInstance.ItemShopTableName, "` set `buyback`='", cost.ToString(), "' where id='", id.ToString(), "';");
                mySqlConnection.Open();
                int affected = mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
                if (affected > 0)
                {
                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                _logger.LogError(null, exception);
                return false;
            }
        }

        public decimal GetItemBuyPrice(int id)
        {
            decimal num = new decimal(0);
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("select `buyback` from `", ZaupShop.Instance.ConfigurationInstance.ItemShopTableName, "` where `id` = '", id.ToString(), "';");
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
                _logger.LogError(null, exception);
            }
            return num;
        }
    }
}