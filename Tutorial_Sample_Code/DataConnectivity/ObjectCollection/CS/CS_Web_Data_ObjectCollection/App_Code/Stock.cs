using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using CrystalDecisions.CrystalReports.Engine;

public class Stock
{
    private double _price;
    private int _volume;
    private Company _company;

	public Stock()
	{

	}
    public double Price
    {
        get
        {
            return _price;
        }
        set
        {
            _price = value;
        }
    }
    public int Volume
    {
        get
        {
            return _volume;
        }
        set
        {
            _volume = value;
        }
    }
    [CrystalDecisions.CrystalReports.Engine.CrystalComplexTypeExpansionLevels(3)]

    public Company Company
    {
        get
        {
            return _company;
        }
        set
        {
            _company = value;
        }
    }

    public Stock(Company company, int volume, double price)
    {
        _company = company;
        _volume = volume;
        _price = price;
    }

}
    public class Company
    {
        private string _symbol;
        private string _name;

        public Company() { }

        public Company(String symbol, String name)
        {
            _symbol = symbol;
            _name = name;
        }

        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
