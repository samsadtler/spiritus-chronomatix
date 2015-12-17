/*
 * Copyright 2014 Google Inc. All Rights Reserved.
 * Distributed under the Project Tango Preview Development Kit (PDK) Agreement.
 * CONFIDENTIAL. AUTHORIZED USE ONLY. DO NOT REDISTRIBUTE.
 */
package com.google.atap.tangoservice;

interface ITangoLogRequestListener {
  void onLogRequest(String category, String action, String label);
}
