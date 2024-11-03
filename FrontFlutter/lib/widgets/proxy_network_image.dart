import 'package:flutter/material.dart';

class ProxyNetworkImage extends StatelessWidget {
  final String imageUrl;
  final BoxFit? fit;
  final double? width;
  final double? height;

  const ProxyNetworkImage({
    super.key,
    required this.imageUrl,
    this.fit,
    this.width,
    this.height,
  });

  String _getProxyUrl() {
    // Using images.weserv.nl as a proxy service
    final encodedUrl = Uri.encodeComponent(imageUrl);
    return 'https://images.weserv.nl/?url=$encodedUrl&default=404';
  }

  @override
  Widget build(BuildContext context) {
    return Image.network(
      _getProxyUrl(),
      fit: fit,
      width: width,
      height: height,
      loadingBuilder: (context, child, loadingProgress) {
        if (loadingProgress == null) return child;
        return Center(
          child: CircularProgressIndicator(
            value: loadingProgress.expectedTotalBytes != null
                ? loadingProgress.cumulativeBytesLoaded /
                    loadingProgress.expectedTotalBytes!
                : null,
          ),
        );
      },
      errorBuilder: (context, error, stackTrace) {
        debugPrint('Error loading image: $error');
        return Container(
          width: width,
          height: height,
          decoration: BoxDecoration(
            color: Colors.grey[200],
            borderRadius: BorderRadius.circular(8),
          ),
          child: const Icon(Icons.image_not_supported, color: Colors.grey),
        );
      },
    );
  }
}