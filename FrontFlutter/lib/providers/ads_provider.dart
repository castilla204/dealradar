import 'dart:convert';
import 'dart:async';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import '../models/ad_model.dart';

class AdsProvider with ChangeNotifier {
  List<AdModel> _ads = [];
  bool _isLoading = false;
  String _error = '';

  List<AdModel> get ads => _ads;
  bool get isLoading => _isLoading;
  String get error => _error;

  Future<void> fetchAds({
    required String keywords,
    required String userSearch,
    required int category,
    required double minPrice,
    required double maxPrice,
  }) async {
    try {
      _isLoading = true;
      _error = '';
      notifyListeners();

      final uri = Uri.parse(
        'https://localhost:7167/WebMixer/GetBestAds'
      ).replace(queryParameters: {
        'keywords': keywords,
        'userSearch': userSearch,
        'pagestoscrape': '3',
        'category': category.toString(),
        'minprice': minPrice.round().toString(),
        'maxprice': maxPrice.round().toString(),
      });

      final response = await http.get(
        uri,
        headers: {
          'Accept': 'application/json',
          'Access-Control-Allow-Origin': '*',
        },
      ).timeout(
        const Duration(minutes: 2), // Increased timeout to 2 minutes
        onTimeout: () {
          throw TimeoutException('The request is taking longer than expected. Please try again.');
        },
      );

      if (response.statusCode == 200) {
        final List<dynamic> data = json.decode(response.body);
        _ads = data.map((json) => AdModel.fromJson(json)).toList();
        if (_ads.isEmpty) {
          _error = 'No results found. Try adjusting your search criteria.';
        }
      } else {
        _error = 'Failed to load ads. Please try again later. (Status: ${response.statusCode})';
        debugPrint('Response body: ${response.body}');
      }
    } catch (e) {
      if (e is TimeoutException) {
        _error = e.toString();
      } else if (e.toString().contains('XMLHttpRequest error')) {
        _error = 'Unable to connect to the server. Please ensure the API server is running and try again.';
      } else {
        _error = 'An unexpected error occurred. Please try again later.';
      }
      debugPrint('Error fetching ads: $e');
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }
}