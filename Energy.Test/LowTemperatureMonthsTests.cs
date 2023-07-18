using Energy.App.Standalone.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Energy.Test
{
    public class LowTemperatureMonthsTests
    {
        [Fact]
        public void Test_GetLowTemperatureDays_NonLeapYear1()
        {
            // Arrange
            DateTime latestDate = new DateTime(2023, 12, 15);

            // Act
            int days = AppWideForecastProperties.GetLowTemperatureDays(latestDate);

            // Assert
            Assert.Equal(212, days); // 31 (Oct) + 30 (Nov) + 31 (Dec) + 31 (Jan) + 28 (Feb) + 31 (Mar) + 30 (Apr) 
        }

        [Fact]
        public void Test_GetLowTemperatureDays_NonLeapYear2()
        {
            // Arrange
            DateTime latestDate = new DateTime(2021, 5, 2);

            // Act
            int days = AppWideForecastProperties.GetLowTemperatureDays(latestDate);

            // Assert
            Assert.Equal(212, days); // 31 (Oct) + 30 (Nov) + 31 (Dec) + 31 (Jan) + 28 (Feb) + 31 (Mar) + 30 (Apr) 
        }

        [Fact]
        public void Test_GetLowTemperatureDays_LeapYear()
        {
            // Arrange
            DateTime latestDate = new DateTime(2024, 12, 15);

            // Act
            int days = AppWideForecastProperties.GetLowTemperatureDays(latestDate);

            // Assert
            Assert.Equal(213, days); // 31 (Oct) + 30 (Nov) + 31 (Dec) + 31 (Jan) + 29 (Feb) + 31 (Mar) + 30 (Apr)
        }
    }
}
