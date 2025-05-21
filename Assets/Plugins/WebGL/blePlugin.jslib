var blePlugin = {
    
    $devices: {},

    
    IsConnected: function (targetName) {
        console.log('>>> isConnect');

        var target = Pointer_stringify(targetName);
        console.log('target:' + target);

        var result = false;

        if (devices[target]) {
            console.log('device:' + devices[target]);
            result = devices[target].gatt.connected;
        }

        console.log('<<< isConnect');

        return result;
    },

    
    Connect: function (targetName) {
        console.log('>>> connect');

        var target = Pointer_stringify(targetName);
        console.log('target:' + target);

        var ACCELEROMETER_SERVICE_UUID = 'e95d0753-251d-470a-a062-fa1922dfa9a8';
        var ACCELEROMETER_DATA_CHARACTERISTIC_UUID = 'e95dca4b-251d-470a-a062-fa1922dfa9a8';
        var ACCELEROMETER_PERIOD_CHARACTERISTIC_UUID = 'e95dfb24-251d-470a-a062-fa1922dfa9a8';
        var BUTTON_SERVICE_UUID = 'e95d9882-251d-470a-a062-fa1922dfa9a8';
        var BUTTON_A_STATE_CHARACTERISTIC_UUID = 'e95dda90-251d-470a-a062-fa1922dfa9a8';
        var BUTTON_B_STATE_CHARACTERISTIC_UUID = 'e95dda91-251d-470a-a062-fa1922dfa9a8';

        var bluetoothServer;
        var accelerometerService;
        var buttonService;

        
        var options = {
            filters: [
                { namePrefix: 'BBC micro:bit' }
            ],
            optionalServices: [ACCELEROMETER_SERVICE_UUID, BUTTON_SERVICE_UUID]
        };
        navigator.bluetooth.requestDevice(options)
            .then(function (device) {
                console.log('id:' + device.id);
                console.log('name:' + device.name);

                
                device.addEventListener('gattserverdisconnected', function (e) {
                    console.log('gattserverdisconnected');
                    SendMessage(target, 'OnDisconnected');
                });

                
                return device.gatt.connect();
            })
            .then(function (server) {
                console.log('connected.');
                devices[target] = server.device;

                
                bluetoothServer = server;
                return bluetoothServer.getPrimaryService(ACCELEROMETER_SERVICE_UUID);
            })
            .then(function (service) {
                console.log('getPrimaryService');

                accelerometerService = service;
                return accelerometerService.getCharacteristic(ACCELEROMETER_PERIOD_CHARACTERISTIC_UUID);
            })
            .then(function (characteristic) {
                console.log('getCharacteristic');

                
                var period = new Uint16Array([20]);
                return characteristic.writeValue(period);
            })
            .then(function () {
                console.log('writeValue');

                return accelerometerService.getCharacteristic(ACCELEROMETER_DATA_CHARACTERISTIC_UUID);
            })
            .then(function (characteristic) {
                console.log('getCharacteristic');

                
                return characteristic.startNotifications();
            })
            .then(function (characteristic) {
                console.log('startNotifications');

                
                characteristic.addEventListener('characteristicvaluechanged', function (ev) {
                    var value = ev.target.value;
                    var x = value.getInt16(0, true);
                    var y = value.getInt16(2, true);
                    var z = value.getInt16(4, true);
                    
                    SendMessage(target, 'OnAccelerometerChangedy', y);
                    SendMessage(target, 'OnAccelerometerChangedx', x);
                    
       
                });

                
                return bluetoothServer.getPrimaryService(BUTTON_SERVICE_UUID);
            })
            .then(function (service) {
                console.log('getPrimaryService');

                buttonService = service;
                return buttonService.getCharacteristic(BUTTON_A_STATE_CHARACTERISTIC_UUID);
            })
            .then(function (characteristic) {
                console.log('getCharacteristic');

                
                return characteristic.startNotifications();
            })
            .then(function (characteristic) {
                console.log('startNotifications');

                
                characteristic.addEventListener('characteristicvaluechanged', function (ev) {
                    var value = ev.target.value;
                    var state = value.getUint8();
                    
                    SendMessage(target, 'OnButtonAChanged', state);
                });

                return buttonService.getCharacteristic(BUTTON_B_STATE_CHARACTERISTIC_UUID);
            })
            .then(function (characteristic) {
                console.log('getCharacteristic');

                
                return characteristic.startNotifications();
            })
            .then(function (characteristic) {
                console.log('startNotifications');

                
                characteristic.addEventListener('characteristicvaluechanged', function (ev) {
                    var value = ev.target.value;
                    var state = value.getUint8();
                    
                    SendMessage(target, 'OnButtonBChanged', state);
                });
            })
            .catch(function (err) {
                console.log('err:' + err);

                if (devices[target]) {
                    if (devices[target].gatt.connected) {
                        devices[target].gatt.disconnect();
                    }
                    delete devices[target];
                }
            });

        console.log('<<< connect');
    },

    
    Disconnect: function (targetName) {
        console.log('>>> disconnect');

        var target = Pointer_stringify(targetName);
        console.log('target:' + target);

        if (devices[target]) {
            console.log('device:' + devices[target]);
            
            devices[target].gatt.disconnect();
            delete devices[target];
        }

        console.log('<<< disconnect');
    }
};
autoAddDeps(blePlugin, '$devices');
mergeInto(LibraryManager.library, blePlugin);
