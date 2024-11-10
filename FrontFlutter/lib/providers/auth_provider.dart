import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:google_sign_in/google_sign_in.dart';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

class AuthProvider with ChangeNotifier {
  final GoogleSignIn _googleSignIn = GoogleSignIn(
    clientId: '61603823707-6rf0724oo0vff1695gnkcspuudt1c9hv.apps.googleusercontent.com',
  );
  
  String? _token;
  Map<String, dynamic>? _userData;
  bool _isLoading = false;
  String _error = '';

  bool get isAuthenticated => _token != null;
  Map<String, dynamic>? get userData => _userData;
  bool get isLoading => _isLoading;
  String get error => _error;

  Future<void> init() async {
    final prefs = await SharedPreferences.getInstance();
    _token = prefs.getString('token');
    final userDataStr = prefs.getString('userData');
    if (userDataStr != null) {
      _userData = json.decode(userDataStr);
    }
    notifyListeners();
  }

  Future<void> signInWithGoogle() async {
    try {
      _isLoading = true;
      _error = '';
      notifyListeners();

      final GoogleSignInAccount? googleUser = await _googleSignIn.signIn();
      if (googleUser == null) {
        throw Exception('Sign in aborted by user');
      }

      final GoogleSignInAuthentication googleAuth = await googleUser.authentication;
      
      final response = await http.post(
        Uri.parse('https://localhost:7167/api/User/google-auth'),
        headers: {'Content-Type': 'application/json'},
        body: json.encode({
          'idToken': googleAuth.idToken,
        }),
      );

      if (response.statusCode == 200) {
        final data = json.decode(response.body);
        _token = data['token'];
        _userData = data['user'];
        
        // Save to local storage
        final prefs = await SharedPreferences.getInstance();
        await prefs.setString('token', _token!);
        await prefs.setString('userData', json.encode(_userData));
      } else {
        throw Exception('Failed to authenticate with server');
      }
    } catch (e) {
      _error = e.toString();
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<void> signOut() async {
    await _googleSignIn.signOut();
    _token = null;
    _userData = null;
    
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('token');
    await prefs.remove('userData');
    
    notifyListeners();
  }
}