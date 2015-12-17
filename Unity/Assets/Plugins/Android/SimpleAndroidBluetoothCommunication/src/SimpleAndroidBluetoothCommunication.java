import java.io.IOException;
import java.io.OutputStream;
import java.util.ArrayList;
import java.util.Set;
import java.util.UUID;

import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Bundle;
//import android.content.Intent;
import android.widget.ArrayAdapter;


public class SimpleAndroidBluetoothCommunication {

	private static final int REQUEST_ENABLE_BT = 1;
	private BluetoothAdapter BA;
	private Set<BluetoothDevice> pairedDevices;
	private ArrayList<String> BTPairedAdapters;
	private ArrayList<String> BTDiscoveredAdapters;
	private ArrayList<BluetoothSocket> BTSockets;
    private ArrayList<OutputStream> BTOutStreams;
	//private ArrayList<CommunicationThread> BTConnections;
	private String deviceAddress;
	private String deviceName;
	private IntentFilter filter;
	private Context context;
	private static SimpleAndroidBluetoothCommunication instance;
	
	// Create a BroadcastReceiver for ACTION_FOUND
	private final BroadcastReceiver mReceiver = new BroadcastReceiver() {
	    public void onReceive(Context context, Intent intent) {
	    	BTDiscoveredAdapters.clear();
	        String action = intent.getAction();
	        // When discovery finds a device
	        if (BluetoothDevice.ACTION_FOUND.equals(action)) {
	            // Get the BluetoothDevice object from the Intent
	            BluetoothDevice device = intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE);
	            // Add the name and address to an array adapter to show in a ListView
	            BTDiscoveredAdapters.add(device.getName() + "\n" + device.getAddress());
	        }
	    }
	};

	public SimpleAndroidBluetoothCommunication() {
		SimpleAndroidBluetoothCommunication.instance = this;
		BTPairedAdapters = new ArrayList<String>();
		BTDiscoveredAdapters = new ArrayList<String>();
		BTSockets = new ArrayList<BluetoothSocket>();
		BTOutStreams = new ArrayList<OutputStream>();
		//BTConnections = new ArrayList<CommunicationThread>();
	}
	
	/*protected void onCreate(Bundle savedInstanceState) {
	    super.onCreate(savedInstanceState);
	    context = this.getApplicationContext();
	}*/
	
    public static SimpleAndroidBluetoothCommunication instance() {
        if(instance == null) {
            instance = new SimpleAndroidBluetoothCommunication();
        }
        return instance;
    }
	
	public String setupAdapter(){
		try{
			BA = BluetoothAdapter.getDefaultAdapter();
			if(BA == null){
				return "NOT SUPPORTED";
			}
		}catch(Exception e){
			return e.toString();
		}
	    
	    return "SUCCESS";
	}
	
	public String adapterOn(){
		try{
			if (!BA.isEnabled()) {
				deviceAddress = BA.getAddress();
			    deviceName = BA.getName();
				//Intent enableBtIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
				//startActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);
			}
		}catch(Exception e){
			return e.toString();
		}
		
    	return "SUCCESS";
	}
	
	public String getDeviceAddress(){
		return deviceAddress;
	}
	
	public String getDeviceName(){
		return deviceName;
	}
	
	public String adapterOff(){
		try{
			BA.disable();
		}catch(Exception e){
			return e.toString();
		}
		
		return "SUCCESS";
	}
	
	public void discoverDevices(){
		// Register the BroadcastReceiver
		filter = new IntentFilter(BluetoothDevice.ACTION_FOUND);
		//registerReceiver(mReceiver, filter);
	}

	public int getPairedDevices(){
		// Clear previously stored paired adapters
		BTPairedAdapters.clear();

		// Get new set of paired devices
		pairedDevices = BA.getBondedDevices();

		// If there are paired devices
		if (pairedDevices.size() > 0) {
		    // Loop through paired devices
		    for (BluetoothDevice device : pairedDevices) {
		        // Add the name and address to paired adapter arraylist (Internal Use)
		        BTPairedAdapters.add(device.getName() + "\n" + device.getAddress());
		        System.out.println("Paired device: " + device.getName() + "\t" + device.getAddress());
		    }
		}
		
		return pairedDevices.size();
	}
	
	public String getPairedDeviceInfo(int index){
		return BTPairedAdapters.get(index);
	}
	
	public String startCommunication(){
		stopCommunication();
		try{
	    	System.out.println("Create 1");
	    	int i = 0;
		    for (BluetoothDevice device : pairedDevices) {
		    	System.out.println("Create 2");
		    	System.out.println("DEVICE: " + device.toString());
		        UUID uuid = device.getUuids()[0].getUuid();
		    	System.out.println("Creating Socket... UUID: " + uuid.toString());
		        // Get a BluetoothSocket to connect with the given BluetoothDevice
		        try {
		            // MY_UUID is the app's UUID string, also used by the server code
		        	System.out.println("CREATING RFCOMM...");
		            BTSockets.add(device.createRfcommSocketToServiceRecord(uuid));
		        } catch (IOException e) {
		        	System.out.println("Socket Creation Failed... " + e.toString());
		        }
		    	//CommunicationThread c = new CommunicationThread(device);
		    	System.out.println("Create 3");
		        try {
		            // Connect the device through the socket. This will block
		            // until it succeeds or throws an exception
		        	System.out.println("Attempting Connection...");
		            BTSockets.get(i).connect();
		        } catch (IOException connectException) {
		            // Unable to connect; close the socket and get out
		        	System.out.println("Connection Failed... " + connectException.toString());
		            try {
		                BTSockets.get(i).close();
		            } catch (IOException closeException) {
		            	System.out.println("Socket closing Failed... " + closeException.toString());
		            }
		        }
		    	System.out.println("Connection Succeeded...");
		    	//c.run();
		    	System.out.println("Create 4");	
		        OutputStream tmpOut = null;
		        
		        // Get the input and output streams, using temp objects because
		        // member streams are final
		        try {
		        	System.out.println("Getting Output Stream");
		            tmpOut = BTSockets.get(i).getOutputStream();
		        } catch (IOException e) {
		        	System.out.println("I/O Error " + e.toString());
		        }
		        BTOutStreams.add(tmpOut);
		    	//BTConnections.add(c);
		    	System.out.println("Create 5");
		    	//break;
		    	i++;
		    }
		}catch(Exception e){
			return e.toString();
		}
		return "SUCCESS";
	}
	
	public void writeChar(String deviceName, char val){
        System.out.println("WRITING TO " + deviceName);
        int i = -1;
        int j = 0;
	    for (BluetoothDevice device : pairedDevices) {
	    	System.out.println(device.getName());
	    	if(device.getName().trim().equals(deviceName)){
	    		System.out.println("DEVICE FOUND");
	    		i = j;
	    	}
	    	j++;
	    }
	    if(i == -1){
	    	System.out.println("Can't find: " + deviceName);
	    	return;
	    }
	    	
       	try{
       		System.out.println(val);
       		BTOutStreams.get(i).write(Character.toString(val).getBytes());
       	}catch (IOException e){
        	System.out.println("ERROR WRITING: " + e.toString());
        }
	}
	
	public void writeString(String deviceName, String val){
        System.out.println("WRITING TO " + deviceName);
        int i = -1;
        int j = 0;
	    for (BluetoothDevice device : pairedDevices) {
	    	System.out.println(device.getName());
	    	if(device.getName().trim().equals(deviceName)){
	    		System.out.println("DEVICE FOUND");
	    		i = j;
	    	}
	    	j++;
	    }
	    if(i == -1){
	    	System.out.println("Can't find: " + deviceName);
	    	return;
	    }
	    	
       	try{
       		System.out.println(val);
       		BTOutStreams.get(i).write(val.getBytes());
       	}catch (IOException e){
        	System.out.println("ERROR WRITING: " + e.toString());
        }
	}
	
	public void stopCommunication(){
		/*for(int i = 0; i < BTConnections.size(); i++){
			BTConnections.get(i).cancel();
			break;
		}
		BTConnections.clear();*/
		for(int i = 0; i < BTSockets.size(); i++){
			System.out.println("Closing Socket " + i);
			try{
				BTSockets.get(i).close();
			}catch (IOException e){
				System.out.println("Error closing socket: " + e.toString());
			}
		}
		BTSockets.clear();
	}
	
    public void onDestroy(){
    	// Kill all threads
    	stopCommunication();
    	
    	// Unregister the BroadcastReceiver
    	//unregisterReceiver(mReceiver); // Don't forget to unregister during onDestroy	
    }
}


class CommunicationThread extends Thread {
	private final BluetoothSocket mmSocket;
    private final BluetoothDevice mmDevice;
    private OutputStream mmOutStream;
    
    public CommunicationThread(BluetoothDevice device) {
        // Use a temporary object that is later assigned to mmSocket,
        // because mmSocket is final
        BluetoothSocket tmp = null;
        mmDevice = device;
        UUID uuid = mmDevice.getUuids()[0].getUuid();
 
    	System.out.println("Creating Socket... UUID: " + uuid.toString());
        // Get a BluetoothSocket to connect with the given BluetoothDevice
        try {
            // MY_UUID is the app's UUID string, also used by the server code
        	System.out.println("CREATING RFCOMM...");
            tmp = device.createRfcommSocketToServiceRecord(uuid);
        } catch (IOException e) {
        	System.out.println("Socket Creation Failed... " + e.toString());
        }
        mmSocket = tmp;
    }
 
    public void run() {
        // Cancel discovery because it will slow down the connection
    	BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.getDefaultAdapter();
    	if (mBluetoothAdapter.isDiscovering()) {
    		mBluetoothAdapter.cancelDiscovery();
    	}
    	
        try {
            // Connect the device through the socket. This will block
            // until it succeeds or throws an exception
        	System.out.println("Attempting Connection...");
            mmSocket.connect();
        } catch (IOException connectException) {
            // Unable to connect; close the socket and get out
        	System.out.println("Connection Failed... " + connectException.toString());
            try {
                mmSocket.close();
            } catch (IOException closeException) { }
            return;
        }
    	System.out.println("Connection Succeeded...");
 
        // Do work to manage the connection (in a separate thread)
        OutputStream tmpOut = null;
 
        // Get the input and output streams, using temp objects because
        // member streams are final
        try {
        	System.out.println("Getting Output Stream");
            tmpOut = mmSocket.getOutputStream();
        } catch (IOException e) {
        	System.out.println("I/O Error " + e.toString());
        	return;
        }
 
        mmOutStream = tmpOut;
        byte[] buffer = new byte[1];
        buffer[0] = 1;
        for(int i = 0; i < 4; i++){
        	System.out.println("WRITING");
        	try{
        		write(buffer[0] == 1 ? "1".getBytes() : "0".getBytes());
        	}catch (IOException e){
            	System.out.println("ERROR WRITING: " + e.toString());
        	}
        	if(buffer[0] == 1){
        		buffer[0] = 0;
        	}else{
        		buffer[0] = 1;
        	}
        	try{
        		sleep(1000);
        	}catch (Exception e){
        		
        	}
        }
        //manageConnectedSocket(mmSocket);
        
        // For now cancel/close
        cancel();
    }
    
    /* Call this from the main activity to send data to the remote device */
    public void write(byte[] bytes) throws IOException{
        mmOutStream.write(bytes);
    }
 
    /** Will cancel an in-progress connection, and close the socket */
    public void cancel() {
        try {
            mmSocket.close();
        } catch (IOException e) { }
    }    
}