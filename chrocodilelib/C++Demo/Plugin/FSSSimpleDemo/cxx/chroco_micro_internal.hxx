#ifndef CHROCO_MICRO_INTERNAL_HXX
#define CHROCO_MICRO_INTERNAL_HXX

#include <string>
#include <vector>
#include <stdexcept>
#include <functional>
#include <array>

#include "CHRocodileLib.h"

namespace chr {

class Connection;

namespace internal {

    inline const char * chrErrorString(Res_t _error)
    {
        thread_local char str[256];
        LibSize_t s = sizeof(str);
        ErrorCodeToString(_error, str, &s);
        return str;
    }

    template <typename T>
    constexpr inline int chooseConst()
    {
        static_assert(sizeof(T) != 0, "T not found in the list of template arguments");
        return 0;
    }

    template <typename T, typename C1, typename... Cs>
    constexpr inline int chooseConst(C1 c1, Cs... constants)
    {
        return std::is_same<T, C1>::value ? static_cast<int>(c1) : chooseConst<T>(constants...);
    }

    template <typename T>
    constexpr inline int16_t chooseSignalType()
    {
        return chooseConst<T>(
            static_cast<int8_t>(Data_Type_Signed_Char),
            static_cast<uint8_t>(Data_Type_Unsigned_Char),
            static_cast<int16_t>(Data_Type_Signed_Short),
            static_cast<uint16_t>(Data_Type_Unsigned_Short),
            static_cast<int32_t>(Data_Type_Signed_Int32),
            static_cast<uint32_t>(Data_Type_Unsigned_Int32),
            static_cast<float>(Data_Type_Float),
            static_cast<double>(Data_Type_Double));
    }


    constexpr inline size_t calcDataSize(int16_t datatype)
    {
        switch (datatype)
        {
        case Data_Type_Unsigned_Char:
        case Data_Type_Signed_Char:
            return 1;
        case Data_Type_Unsigned_Short:
        case Data_Type_Signed_Short:
            return 2;
        case Data_Type_Unsigned_Int32:
        case Data_Type_Signed_Int32:
        case Data_Type_Float:
            return 4;
        case Data_Type_Double:
            return 8;
        default:
            break;
        }
        return 0;
    }

    class DataSamplesReader;
    class DataSample;

    template< class T >
    class StridedIterator
    {
        friend class DataSamplesReader;
        friend class DataSample;

        StridedIterator(const uint8_t *dataPtr, const size_t stride)
            : m_dataPtr(dataPtr), m_stride(stride)
        {   }

    public:
        using iterator_category = std::forward_iterator_tag;
        using value_type = T;
        using difference_type = std::ptrdiff_t;
        using pointer = T * ;
        using reference = T & ;

        const T& operator*() const {
            return *static_cast<const T *>(m_dataPtr);
        }
        const T* operator ->() const {
            return static_cast<const T *>(m_dataPtr);
        }
        StridedIterator &operator++()
        {
            m_dataPtr += m_stride;
            return *this;
        }

        bool operator==(const StridedIterator &rhs) const
        {
            return m_dataPtr == rhs.m_dataPtr && m_stride == rhs.m_stride;
        }
        bool operator!=(const StridedIterator &rhs) const { return not operator==(rhs); }

    private:
        const uint8_t *m_dataPtr;
        const size_t m_stride;
    };


#if 0 // recursive data structure similar to tuple but with clear layoyt..
        template<typename ... T>
        struct DataStructure {};

        template<size_t idx, typename ZT, typename ... ZRest>
        auto ZZget(DataStructure<ZT, ZRest...>& data)
        {
            if constexpr (idx == 0)
                return data.first;
            else
                return ZZget<idx - 1, ZRest...>(data.rest);
        }

        template<typename T, typename ... Rest>
        struct DataStructure<T, Rest...>
        {

            DataStructure(const T& first, const Rest& ... rest)
                : first(first)
                , rest(rest...)
            {}

            template<size_t idx>
            auto get()
            {
                return ZZget<idx, T, Rest...>(*this);
            }

            T first;
            DataStructure<Rest ... > rest;
        };
#endif

    class DataSamplesReader
    {
    private:

        DataSamplesReader() = default;

        struct SignalInfo {
            size_t byteOfs, dataSize;
            uint16_t signalID;
            int16_t dataType;
        };

    public:
        friend class chr::Connection;

        // raw data access
        const uint8_t *data() const { return m_dataPtr; }
        LibSize_t sampleSize() const { return m_sampleSize; }
        LibSize_t sampleCount() const { return m_sampleCount; }

        size_t numGlobalSignals() const { return m_genSigInfo.GlobalSignalCount; }
        size_t numPeakSignals() const { return m_genSigInfo.PeakSignalCount; }
        size_t channelCount() const { return m_genSigInfo.ChannelCount; }

        // get the position of a specific singal in a data sample
        int32_t signalIndex(uint16_t signalID) const {
            for (int i = 0; i < (int)m_sigInfoVec.size(); i++) {
                if (m_sigInfoVec[i].signalID == signalID) {
                    int j = i - m_genSigInfo.GlobalSignalCount;
                    return (j < 0 ? i : j); // subtract global signal count for peak signals..
                }
            }
            return -1;
        }

        template<class T, uint32_t Num>
        decltype(auto) checkPack(uint32_t startI) const
        {
            if (startI >= m_sigInfoVec.size())
                throw std::runtime_error("Too many global signals requested!");

            const auto& s = m_sigInfoVec[startI];
            if (chooseSignalType<T>() != s.dataType)
                throw std::runtime_error("Wrong data type requested for a global signal ID#" + std::to_string(s.signalID));

            if constexpr (Num > 1)
                checkPack<T, Num - 1>(startI + 1);
            return s;
        }

        // returns a tuple of global signals in range [startI; startI + num]
        template<class T, uint32_t Num>
        decltype(auto) globalPack(uint32_t startI) const
        {
            const auto& s = checkPack<T, Num>(startI);
            using Typ = std::array<T, Num>;

            size_t stride = m_sampleSize;
            auto pbegin = m_dataPtr + s.byteOfs, pend = pbegin + stride * m_sampleCount;

            return std::make_tuple(StridedIterator< Typ >(pbegin, stride), StridedIterator< Typ >(pend, stride));
        }

        class DataSample
        {
            DataSamplesReader& m_reader;
            const uint8_t *m_dataBegin;

            friend class DataSamplesReader;

        private:
            DataSample(DataSamplesReader& reader, const uint8_t *dataBegin)
                : m_reader(reader)
                , m_dataBegin(dataBegin)
            { }

            template<class T>
            using IterPack = std::tuple< StridedIterator<T>, StridedIterator<T>>;

        public:

            template<class T>
            T globalBySignalID(uint16_t sigID) const {
                return global<T>(m_reader.signalIndex(sigID));
            }

            template<class T>
            T global(uint32_t index) const {
                //        if (index >= (uint32_t)m_reader.m_genSigInfo.GlobalSignalCount)
                //            throw std::runtime_error("No global signal #" + std::to_string(index) + " given in this sample!");

                const auto& s = m_reader.m_sigInfoVec[index];
                //if (chooseSignalType<T>() != s.dataType)
                //    throw std::runtime_error("Wrong data type requested for a global signal ID#" + std::to_string(s.signalID));

                return *(const T *)(m_dataBegin + s.byteOfs);
            }

            template<class T>
            IterPack<T> peakBySignalID(uint16_t sigID) const {
                return peak<T>(m_reader.signalIndex(sigID));
            }

            template<class T>
            IterPack<T> peak(uint32_t index) const { // peaks signals are stored on top of the global signals
        //        if(index >= (uint32_t)m_reader.m_genSigInfo.PeakSignalCount) {
        //            throw std::runtime_error("No peak signal #" + std::to_string(index) + " given in this sample!");
        //        }

                const auto& s = m_reader.m_sigInfoVec[index];
                //if (chooseSignalType<T>() != s.dataType)
                //    throw std::runtime_error("Wrong data type requested for a peak signal ID#" + std::to_string(s.signalID));

                size_t stride = m_reader.m_peakSize;
                auto pbegin = m_dataBegin + s.byteOfs, pend = pbegin + stride * m_reader.m_genSigInfo.PeakSignalCount;

                return { StridedIterator< T >(pbegin, stride), StridedIterator< T >(pend, stride) };
            }
        };

        class Iterator
        {
            using iterator_category = std::forward_iterator_tag;
            using value_type = DataSample; // crap
            using difference_type = std::ptrdiff_t;
            using pointer = DataSample * ;
            using reference = DataSample & ;

            friend class DataSamplesReader;

            DataSamplesReader& m_self;
            const uint8_t *m_samplePtr;

            Iterator(DataSamplesReader& self, const uint8_t *samplePtr)
                : m_self(self), m_samplePtr(samplePtr)
            {  }

        public:
            Iterator &operator++()
            {
                m_samplePtr += m_self.m_sampleSize;
                return *this;
            }
            DataSample operator*() {
                return DataSample(m_self, m_samplePtr);
            }
            bool operator==(const Iterator &_other) const
            {
                return m_samplePtr == _other.m_samplePtr && &m_self == &_other.m_self;
            }
            bool operator!=(const Iterator &_other) const { return !operator==(_other); }
        };

        Iterator begin() { return Iterator(*this, data()); }
        Iterator end() { return Iterator(*this, data() + sampleCount() * m_sampleSize); }

    private:
        void reset() {
            m_genSigInfo = {};
            m_sampleCount = 0, m_dataPtr = nullptr;
            m_sampleSize = 0, m_peakSize = 0;
        }

        void update(const TSampleSignalGeneralInfo & genInfo, const TSampleSignalInfo *psigInfo,
            const uint8_t *dataPtr, int64_t sampleCount, LibSize_t sampleSize)
        {
            if (genInfo.InfoIndex != m_genSigInfo.InfoIndex) { // update signal definitions

                m_genSigInfo = genInfo;
                m_sigInfoVec.resize(genInfo.GlobalSignalCount + genInfo.PeakSignalCount);

                size_t byteOfs = 0;
                for (auto& s : m_sigInfoVec)
                {
                    s.byteOfs = byteOfs;
                    s.dataSize = calcDataSize(psigInfo->DataType);
                    s.signalID = psigInfo->SignalID;
                    s.dataType = psigInfo->DataType;
                    //                std::cerr << "updateDataFormat: " << psigInfo->SignalID << " - dataTyp: " << psigInfo->DataType << " sz: " <<
                    //                             s.dataSize << " byteOfs: " << byteOfs << std::endl;

                    byteOfs += s.dataSize, psigInfo++;
                }
                // this makes sense only for multi-channel devices => we know that global signals go first
                if (genInfo.PeakSignalCount > 0) {
                    m_peakSize = byteOfs - m_sigInfoVec[genInfo.GlobalSignalCount].byteOfs;
                }
            } // if InfoIndex

            m_dataPtr = dataPtr;
            m_sampleSize = sampleSize, m_sampleCount = sampleCount;
        }

        TSampleSignalGeneralInfo m_genSigInfo = {};
        LibSize_t m_sampleCount = 0, m_sampleSize = 0,
            m_peakSize = 0; // size in bytes of all peak signals in a sample
        const uint8_t *m_dataPtr = nullptr;
        std::vector< SignalInfo > m_sigInfoVec;
    };

} // namespace internal

} // namespace chr

#endif // CHROCO_MICRO_INTERNAL_HXX
