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
  final String category;
  final int categoryId;
  final String province;
  final int provinceId;
  final int cityId;
  final bool highlighted;
  final bool isNew;
  final String isReserved;
  final String slug;
  final String sellerType;
  final List<String> tags;
  final int userId;
  final DateTime scrappedDate;
  final String url;
  final int adScore;
  final int finalScore;

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
    required this.category,
    required this.categoryId,
    required this.province,
    required this.provinceId,
    required this.cityId,
    required this.highlighted,
    required this.isNew,
    required this.isReserved,
    required this.slug,
    required this.sellerType,
    required this.tags,
    required this.userId,
    required this.scrappedDate,
    required this.url,
    required this.adScore,
    required this.finalScore,
  });

  factory AdModel.fromJson(Map<String, dynamic> json) {
    return AdModel(
      id: json['id'] ?? '',
      title: json['title'] ?? '',
      description: json['description'] ?? '',
      images: List<String>.from(json['images'] ?? []),
      price: (json['price'] ?? 0).toDouble(),
      city: json['city'] ?? '',
      goodThings: List<String>.from(json['goodThings'] ?? []),
      badThings: List<String>.from(json['badThings'] ?? []),
      publishDate: DateTime.parse(json['publishDate'] ?? DateTime.now().toIso8601String()),
      updateDate: DateTime.parse(json['updateDate'] ?? DateTime.now().toIso8601String()),
      category: json['category'] ?? '',
      categoryId: json['categoryId'] ?? 0,
      province: json['province'] ?? '',
      provinceId: json['provinceId'] ?? 0,
      cityId: json['cityId'] ?? 0,
      highlighted: json['highlighted'] ?? false,
      isNew: json['isNew'] ?? false,
      isReserved: json['isReserved'] ?? '',
      slug: json['slug'] ?? '',
      sellerType: json['sellerType'] ?? '',
      tags: List<String>.from(json['tags'] ?? []),
      userId: json['userId'] ?? 0,
      scrappedDate: DateTime.parse(json['scrappedDate'] ?? DateTime.now().toIso8601String()),
      url: json['url'] ?? '',
      adScore: json['adScore'] ?? 0,
      finalScore: json['finalScore'] ?? 0,
    );
  }
}