IF NOT EXISTS (SELECT Id FROM DeviceVendors WHERE VendorName = @Vendor) INSERT INTO DeviceVendors OUTPUT inserted.Id VALUES(@Vendor) ELSE SELECT Id FROM DeviceVendors WHERE VendorName = @Vendor

IF NOT EXISTS (SELECT Id FROM DeviceModels WHERE ModelName = @ModelName)INSERT INTO DeviceModels OUTPUT inserted.Id VALUES(@ModelName, @VendorId) ELSE SELECT Id FROM DeviceModels WHERE ModelName = @ModelName

IF NOT EXISTS (SELECT Id FROM DeviceTypes WHERE TypeName = @TypeName) INSERT INTO DeviceTypes OUTPUT inserted.Id VALUES(@TypeName) ELSE SELECT Id FROM DeviceTypes WHERE TypeName = @TypeName

IF NOT EXISTS (SELECT Id FROM GeoLocations WHERE Latitude = @Latitude AND Longitude = @Longitude) INSERT INTO GeoLocations OUTPUT inserted.Id VALUES(@Latitude, @Longitude) ELSE SELECT Id FROM GeoLocations WHERE Latitude = @Latitude AND Longitude = @Longitude

IF NOT EXISTS (SELECT DeviceName FROM Devices WHERE DeviceName = @DeviceName) INSERT INTO Devices OUTPUT inserted.DeviceName VALUES(@DeviceName, @DeviceTypeId, @GeoLocationId, @ModelId) ELSE SELECT DeviceName FROM Devices WHERE DeviceName = @DeviceName

SELECT * FROM Devices
SELECT * FROM DeviceTypes
SELECT * FROM DeviceModels
SELECT * FROM DeviceVendors
SELECT * FROM GeoLocations

DELETE FROM DeviceVendors