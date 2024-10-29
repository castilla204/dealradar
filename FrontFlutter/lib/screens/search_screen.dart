import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import '../widgets/filter_chip.dart';
import '../widgets/product_card.dart';

class SearchScreen extends StatefulWidget {
  const SearchScreen({super.key});

  @override
  State<SearchScreen> createState() => _SearchScreenState();
}

class _SearchScreenState extends State<SearchScreen> {
  final TextEditingController _searchController = TextEditingController();
  RangeValues _priceRange = const RangeValues(0, 1000);
  String _selectedCategory = 'All';
  String _selectedCondition = 'All';
  final _scrollController = ScrollController();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: AnnotatedRegion<SystemUiOverlayStyle>(
        value: SystemUiOverlayStyle.dark,
        child: SafeArea(
          child: Column(
            children: [
              Container(
                padding: const EdgeInsets.all(16.0),
                decoration: BoxDecoration(
                  color: Colors.grey[50],
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withOpacity(0.05),
                      blurRadius: 10,
                      offset: const Offset(0, 2),
                    ),
                  ],
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'Find Your Next Treasure',
                      style: TextStyle(
                        fontSize: 24,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 16),
                    TextField(
                      controller: _searchController,
                      decoration: InputDecoration(
                        hintText: 'Search with AI-powered recommendations...',
                        hintStyle: TextStyle(color: Colors.grey[400]),
                        prefixIcon: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            const SizedBox(width: 12),
                            Icon(Icons.search, color: Colors.indigo[400]),
                            const SizedBox(width: 4),
                            Container(
                              padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                              decoration: BoxDecoration(
                                color: Colors.indigo[50],
                                borderRadius: BorderRadius.circular(4),
                              ),
                              child: Row(
                                mainAxisSize: MainAxisSize.min,
                                children: [
                                  Icon(Icons.auto_awesome,
                                    size: 14,
                                    color: Colors.indigo[400],
                                  ),
                                  const SizedBox(width: 2),
                                  Text(
                                    'AI',
                                    style: TextStyle(
                                      fontSize: 12,
                                      fontWeight: FontWeight.w600,
                                      color: Colors.indigo[400],
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ],
                        ),
                        suffixIcon: IconButton(
                          icon: const Icon(Icons.tune, color: Colors.indigo),
                          onPressed: () => _showFilterBottomSheet(context),
                        ),
                      ),
                    ),
                    const SizedBox(height: 16),
                    SingleChildScrollView(
                      scrollDirection: Axis.horizontal,
                      child: Row(
                        children: [
                          CustomFilterChip(
                            label: 'All',
                            isSelected: _selectedCategory == 'All',
                            onSelected: (value) => setState(() => _selectedCategory = 'All'),
                          ),
                          const SizedBox(width: 8),
                          CustomFilterChip(
                            label: 'Electronics',
                            isSelected: _selectedCategory == 'Electronics',
                            onSelected: (value) => setState(() => _selectedCategory = 'Electronics'),
                          ),
                          const SizedBox(width: 8),
                          CustomFilterChip(
                            label: 'Furniture',
                            isSelected: _selectedCategory == 'Furniture',
                            onSelected: (value) => setState(() => _selectedCategory = 'Furniture'),
                          ),
                          const SizedBox(width: 8),
                          CustomFilterChip(
                            label: 'Clothing',
                            isSelected: _selectedCategory == 'Clothing',
                            onSelected: (value) => setState(() => _selectedCategory = 'Clothing'),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
              Expanded(
                child: GridView.builder(
                  controller: _scrollController,
                  padding: const EdgeInsets.all(8),
                  gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                    crossAxisCount: 2,
                    childAspectRatio: 0.8,
                    crossAxisSpacing: 8,
                    mainAxisSpacing: 8,
                  ),
                  itemCount: 10,
                  itemBuilder: (context, index) => ProductCard(
                    onTap: () {
                      // Add product detail navigation here
                    },
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  void _showFilterBottomSheet(BuildContext context) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => StatefulBuilder(
        builder: (context, setState) => Container(
          decoration: const BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
          ),
          padding: EdgeInsets.fromLTRB(20, 20, 20, MediaQuery.of(context).viewInsets.bottom + 20),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  const Text(
                    'Filters',
                    style: TextStyle(
                      fontSize: 20,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  IconButton(
                    icon: const Icon(Icons.close),
                    onPressed: () => Navigator.pop(context),
                  ),
                ],
              ),
              const SizedBox(height: 20),
              const Text(
                'Price Range',
                style: TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.w600,
                ),
              ),
              const SizedBox(height: 8),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    '\$${_priceRange.start.round()}',
                    style: const TextStyle(fontWeight: FontWeight.w500),
                  ),
                  Text(
                    '\$${_priceRange.end.round()}',
                    style: const TextStyle(fontWeight: FontWeight.w500),
                  ),
                ],
              ),
              RangeSlider(
                values: _priceRange,
                min: 0,
                max: 1000,
                divisions: 20,
                activeColor: Colors.indigo,
                inactiveColor: Colors.grey[300],
                labels: RangeLabels(
                  '\$${_priceRange.start.round()}',
                  '\$${_priceRange.end.round()}',
                ),
                onChanged: (values) => setState(() => _priceRange = values),
              ),
              const SizedBox(height: 20),
              const Text(
                'Condition',
                style: TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.w600,
                ),
              ),
              const SizedBox(height: 12),
              Wrap(
                spacing: 8,
                runSpacing: 8,
                children: [
                  CustomFilterChip(
                    label: 'New',
                    isSelected: _selectedCondition == 'New',
                    onSelected: (value) => setState(() => _selectedCondition = 'New'),
                  ),
                  CustomFilterChip(
                    label: 'Like New',
                    isSelected: _selectedCondition == 'Like New',
                    onSelected: (value) => setState(() => _selectedCondition = 'Like New'),
                  ),
                  CustomFilterChip(
                    label: 'Good',
                    isSelected: _selectedCondition == 'Good',
                    onSelected: (value) => setState(() => _selectedCondition = 'Good'),
                  ),
                  CustomFilterChip(
                    label: 'Fair',
                    isSelected: _selectedCondition == 'Fair',
                    onSelected: (value) => setState(() => _selectedCondition = 'Fair'),
                  ),
                ],
              ),
              const SizedBox(height: 24),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  onPressed: () => Navigator.pop(context),
                  child: const Text(
                    'Apply Filters',
                    style: TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}