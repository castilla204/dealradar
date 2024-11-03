class AdModel {
  final String id;
  final String title;
  final String description;
  final List<String> images;
  final double price;
  final String city;
  final List<String> goodThings;
  final List<String> badThings;
  final DateTime publishDate;
  final DateTime updateDate;

  AdModel({
    required this.id,
    required this.title,
    required this.description,
    required this.images,
    required this.price,
    required this.city,
    required this.goodThings,
    required this.badThings,
    required this.publishDate,
    required this.updateDate,
  });

  factory AdModel.fromJson(Map<String, dynamic> json) {
    return AdModel(
      id: json['id'] ?? '',
      title: json['title'] ?? '',
      description: json['description'] ?? '',
      images: List<String>.from(json['images'] ?? []),
      price: (json['price']?['cashPrice']?['value'] ?? 0).toDouble(),
      city: json['city']?['name'] ?? '',
      goodThings: List<String>.from(json['goodThings'] ?? []),
      badThings: List<String>.from(json['badThings'] ?? []),
      publishDate: DateTime.parse(json['publishDate'] ?? DateTime.now().toIso8601String()),
      updateDate: DateTime.parse(json['updateDate'] ?? DateTime.now().toIso8601String()),
    );
  }
}